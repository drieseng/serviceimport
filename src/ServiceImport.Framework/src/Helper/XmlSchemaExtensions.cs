using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;

namespace ServiceImport.Framework.Helper
{
    internal static class XmlSchemaExtensions
    {
        public static IEnumerable<XmlSchemaComplexType> ComplexTypes(this XmlSchemaSet xmlSchemaSet)
        {
            foreach (XmlSchema xmlSchema in xmlSchemaSet.Schemas())
                foreach (var complexType in xmlSchema.ComplexTypes())
                    yield return complexType;
        }

        public static IEnumerable<XmlSchemaComplexType> ComplexTypes(this XmlSchema xmlSchema)
        {
            foreach (var schemaType in xmlSchema.SchemaTypes)
            {
                var schemaTypeEntry = (DictionaryEntry) schemaType;

                var complexType = schemaTypeEntry.Value as XmlSchemaComplexType;
                if (complexType != null)
                    yield return complexType;
            }

            foreach (var elementObject in xmlSchema.Elements)
            {
                var elementEntry = (DictionaryEntry) elementObject;

                var element = elementEntry.Value as XmlSchemaElement;
                if (element == null)
                    continue;

                foreach (var elementComplexType in GetComplexTypes(element))
                    yield return elementComplexType;
            }
        }

        private static IEnumerable<XmlSchemaComplexType> GetComplexTypes(XmlSchemaElement element)
        {
            if (!element.SchemaTypeName.IsEmpty)
                yield break;

            var complexType = element.SchemaType as XmlSchemaComplexType;
            if (complexType == null)
                yield break;

            yield return complexType;

            var particle = complexType.Particle;
            if (particle == null)
                yield break;

            var schemaSequence = particle as XmlSchemaSequence;
            if (schemaSequence == null)
                yield break;

            foreach (var item in schemaSequence.Items)
            {
                var elementItem = item as XmlSchemaElement;
                if (elementItem == null)
                    throw new ArgumentException(
                        $"The sequence of complex type '{complexType.QualifiedName}' should only contain elements.");

                foreach (var elementComplexType in GetComplexTypes(elementItem))
                    yield return elementComplexType;
            }
        }
    }
}
