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

        public static bool Contains(this XmlSchema xmlSchema, XmlSchemaSimpleType simpleType)
        {
            foreach (var schemaObject in xmlSchema.Items)
            {
                if (schemaObject is XmlSchemaSimpleType type && type.QualifiedName == simpleType.QualifiedName)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool Contains(this XmlSchema xmlSchema, XmlSchemaComplexType complexType)
        {
            foreach (var schemaObject in xmlSchema.Items)
            {
                if (schemaObject is XmlSchemaComplexType type && type.QualifiedName == complexType.QualifiedName)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool Contains(this XmlSchema xmlSchema, XmlSchemaElement element)
        {
            foreach (var schemaObject in xmlSchema.Items)
            {
                if (schemaObject is XmlSchemaElement schemaElement && schemaElement.QualifiedName == element.QualifiedName)
                {
                    return true;
                }
            }

            return false;
        }

        public static IEnumerable<XmlSchemaComplexType> ComplexTypes(this XmlSchema xmlSchema)
        {
            foreach (var schemaType in xmlSchema.SchemaTypes)
            {
                var schemaTypeEntry = (DictionaryEntry) schemaType;

                if (schemaTypeEntry.Value is XmlSchemaComplexType complexType)
                    yield return complexType;
            }

            foreach (var elementObject in xmlSchema.Elements)
            {
                var elementEntry = (DictionaryEntry) elementObject;

                if (!(elementEntry.Value is XmlSchemaElement element))
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

            if (!(particle is XmlSchemaSequence schemaSequence))
                yield break;

            foreach (var item in schemaSequence.Items)
            {
                if (!(item is XmlSchemaElement elementItem))
                    throw new ArgumentException(
                        $"The sequence of complex type '{complexType.QualifiedName}' should only contain elements.");

                foreach (var elementComplexType in GetComplexTypes(elementItem))
                    yield return elementComplexType;
            }
        }
    }
}
