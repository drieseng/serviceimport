using System;
using System.Collections.Generic;
using System.ServiceModel.Description;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Schema;
using SWSServiceDescription = System.Web.Services.Description.ServiceDescription;

namespace ServiceImport.Framework.Helper
{
    internal static class WsdlImporterExtensions
    {
        private static readonly XmlQualifiedName StringQName = new XmlQualifiedName("string", XmlSchema.Namespace);

        public static XmlSchemaSet MergeSchemas(this ServiceDescriptionCollection wsdlDocuments, XmlSchemaSet xmlSchemas)
        {
            var allSchemas = new XmlSchemaSet();
            foreach (SWSServiceDescription wsdlDoc in wsdlDocuments)
                foreach (XmlSchema schema in wsdlDoc.Types.Schemas)
                    allSchemas.Add(schema);
            foreach (XmlSchema schema in xmlSchemas.Schemas())
                allSchemas.Add(schema);

            return NormalizeSchemas(allSchemas);
        }

        /// <summary>
        /// Normalized the XML schemas in the specified <see cref="XmlSchemaSet"/>.
        /// </summary>
        /// <param name="schemaSet">The <see cref="XmlSchemaSet"/> to normalize.</param>
        /// <returns>
        /// The normalized <see cref="XmlSchemaSet"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Merges schema objects from XML schemas with the same target namespace.
        /// </para>
        /// <para>
        /// A <see cref="XmlSchemaSet"/> can contain more than one XML schemas for the same target namespace because:
        /// <list type="bullet">
        ///   <item>
        ///     <description>
        ///     Duplicates are introduced when <see cref="XmlSchemaSet"/> resolves imports.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///     Multiple WSDLs can define an inline XML schema with the same target namespace. These must be united
        ///     to ensure the elements and types from all these XML schemas are available to the importer.
        ///     </description>
        ///   </item>
        /// </list>
        /// </para>
        /// </remarks>
        private static XmlSchemaSet NormalizeSchemas(XmlSchemaSet schemaSet)
        {
            var normalizedSchemas = new Dictionary<string, XmlSchema>();

            foreach (XmlSchema schema in schemaSet.Schemas())
            {
                if (!normalizedSchemas.TryGetValue(schema.TargetNamespace, out var normalizedSchema))
                {
                    normalizedSchema = new XmlSchema();
                    normalizedSchema.TargetNamespace = schema.TargetNamespace;
                    normalizedSchemas.Add(schema.TargetNamespace, normalizedSchema);
                }

                foreach (var schemaObject in schema.Items)
                {
                    if (schemaObject is XmlSchemaElement element)
                    {
                        if (!normalizedSchema.Contains(element))
                        {
                            normalizedSchema.Items.Add(element);
                        }
                    }
                    else if (schemaObject is XmlSchemaSimpleType simpleType)
                    {
                        if (!normalizedSchema.Contains(simpleType))
                        {
                            normalizedSchema.Items.Add(simpleType);
                        }
                    }
                    else if (schemaObject is XmlSchemaComplexType complexType)
                    {
                        if (!normalizedSchema.Contains(complexType))
                        {
                            normalizedSchema.Items.Add(complexType);
                        }
                    }
                }
            }

            var normalizedSchemaSet = new XmlSchemaSet();

            foreach (var kvp in normalizedSchemas)
            {
                normalizedSchemaSet.Add(kvp.Value);
            }

            normalizedSchemaSet.Compile();
            return normalizedSchemaSet;
        }


        public static IEnumerable<XmlSchemaElement> GetWrapperElementParameters(this XmlSchemaElement wrapperElement)
        {
            var schemaType = wrapperElement.SchemaType;
            if (schemaType == null)
                throw new Exception(string.Format("Wrapped element '{0}' does not define its own schema type.", wrapperElement.QualifiedName));

            var schemaComplexType = schemaType as XmlSchemaComplexType;
            if (schemaComplexType == null)
                throw new Exception(string.Format("Wrapped element '{0}' does not define a complex type.", wrapperElement.QualifiedName));

            var schemaParticle = schemaComplexType.Particle;
            if (schemaParticle == null)
                throw new Exception(string.Format("Complex type for wrapped element '{0}' does not define a particle.", wrapperElement.QualifiedName));

            var schemaSequence = schemaParticle as XmlSchemaSequence;
            if (schemaSequence == null)
                throw new Exception(string.Format("Complex type for wrapped element '{0}' does not define a sequence.", wrapperElement.QualifiedName));

            foreach (var item in schemaSequence.Items)
            {
                var elementItem = item as XmlSchemaElement;
                if (elementItem == null)
                    throw new Exception(string.Format("The sequence for wrapped element '{0}' should only contain elements.", wrapperElement.QualifiedName));

                if (elementItem.RefName != null && !elementItem.RefName.IsEmpty)
                    throw new Exception(string.Format("Element '{0}' for wrapped element '{1}' should not reference a global element.", elementItem.Name, wrapperElement.QualifiedName));

                //if (elementItem.SchemaType != null)
                //    throw new Exception(string.Format("Element '{0}' for wrapped element '{1}' does not reference a global type.", elementItem.Name, wrapperElement.QualifiedName));

                if (elementItem.SchemaTypeName != null)
                    yield return elementItem;
            }
        }

        public static IEnumerable<XmlSchemaElement> GetSequenceElements(this XmlSchemaComplexType complexType)
        {
            var contentTypeParticle = complexType.ContentTypeParticle;
            if (contentTypeParticle != null)
            {
                var schemaSequence = contentTypeParticle as XmlSchemaSequence;
                if (schemaSequence != null)
                {
                    foreach (var item in schemaSequence.Items)
                    {
                        var elementItem = item as XmlSchemaElement;
                        if (elementItem == null)
                            throw new ArgumentException($"The sequence of complex type '{complexType.QualifiedName}' should only contain elements.");

                        yield return elementItem;
                    }

                    yield break;
                }
            }

            var particle = complexType.Particle;
            if (particle != null)
            {
                var schemaSequence = particle as XmlSchemaSequence;
                if (schemaSequence != null)
                {
                    foreach (var item in schemaSequence.Items)
                    {
                        var elementItem = item as XmlSchemaElement;
                        if (elementItem == null)
                            throw new ArgumentException($"The sequence of complex type '{complexType.QualifiedName}' should only contain elements.");

                        yield return elementItem;
                    }

                    yield break;
                }
            }

            throw new ArgumentException(string.Format("Complex type '{0}' does not define a particle.", complexType.QualifiedName));
        }

        public static T GetOrCreate<T>(this WsdlImporter wsdlImporter, Func<T> createMethod)
        {
            T typedExtension;

            object extension;
            wsdlImporter.State.TryGetValue(typeof(T), out extension);
            if (extension == null)
            {
                typedExtension = createMethod.Invoke();
                wsdlImporter.State.Add(typeof(T), typedExtension);
            }
            else
            {
                typedExtension = (T)extension;
            }

            return typedExtension;
        }

        public static T Get<T>(this WsdlImporter wsdlImporter) where T : class
        {
            T typedExtension;

            object extension;
            wsdlImporter.State.TryGetValue(typeof(T), out extension);
            if (extension != null)
            {
                typedExtension = (T)extension;
            }
            else
            {
                typedExtension = null;
            }

            return typedExtension;
        }

        public static bool IsReferenceType(this XmlSchemaElement element)
        {
            var schemaType = element.ElementSchemaType;
            if (schemaType == null)
                throw new ArgumentException(string.Format("ElementSchemaType for '{0}' is null.", element.Name), "element");

            if (schemaType is XmlSchemaComplexType)
                return true;

            if (!(schemaType is XmlSchemaSimpleType simpleType))
                return true;

            if (!simpleType.QualifiedName.IsEmpty && simpleType.QualifiedName.Namespace == "http://schemas.microsoft.com/2003/10/Serialization/")
            {
                switch (simpleType.Name)
                {
                    case "guid":
                        // will be mapped to System.Guid
                        return false;
                    case "char":
                        // will be mapped to System.Char
                        return false;
                }
            }

            switch (simpleType.TypeCode)
            {
                case XmlTypeCode.String:
                    return !IsEnumeration(simpleType);
                case XmlTypeCode.Base64Binary:
                    // will be mapped to a byte array
                    return true;
                case XmlTypeCode.Date:
                    // will be mapped to a string
                    return true;
                case XmlTypeCode.Time:
                    // will be mapped to a string
                    return true;
                case XmlTypeCode.Int:
                    return false;
                case XmlTypeCode.Long:
                    return false;
                case XmlTypeCode.Integer:
                    return false;
                case XmlTypeCode.DateTime:
                    return false;
                case XmlTypeCode.Double:
                    return false;
                case XmlTypeCode.Decimal:
                    return false;
                case XmlTypeCode.Float:
                    return false;
                case XmlTypeCode.NonNegativeInteger:
                    return false;
                case XmlTypeCode.PositiveInteger:
                    return false;
                case XmlTypeCode.NonPositiveInteger:
                    return false;
                case XmlTypeCode.Boolean:
                    return false;
                default:
                    throw new NotSupportedException($"XML type code '{simpleType.TypeCode}' (in {element.QualifiedName}) is not supported");
            }
        }

        private static bool IsEnumeration(XmlSchemaSimpleType simpleType)
        {
            if (!(simpleType.Content is XmlSchemaSimpleTypeRestriction restriction))
                return false;

            // only simple types with a restriction that has xs:string as base can be mapped to a .NET enumeration
            if (restriction.BaseTypeName != StringQName)
                return false;

            // a restriction with zero facets is not mapped to a .NET enumeration
            if (restriction.Facets.Count == 0)
                return false;

            // if there are restriction facets, then they all have to be enumeration facets
            foreach (var facet in restriction.Facets)
                if (!(facet is XmlSchemaEnumerationFacet))
                    return false;

            return true;
        }
    }
}
