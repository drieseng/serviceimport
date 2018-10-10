using System.Web.Services.Description;
using System.Xml.Schema;
using NUnit.Framework;
using ServiceImport.Framework.Helper;

namespace ServiceImport.Framework.Tests.Helper
{
    [TestFixture]
    public class WsdlImporterExtensionsTests
    {
        [Test]
        public void MergeSchemas()
        {
            var wsdlDocuments = new ServiceDescriptionCollection();

            var schemaA1 = CreateXmlSchema("A", string.Empty);
            var schemaB1 = CreateXmlSchema("B", string.Empty);
            var schemaB2 = CreateXmlSchema("B", string.Empty);
            var schemaC1 = CreateXmlSchema("C", "http://test/schemaC");
            var schemaA2 = CreateXmlSchema("A", "http://test/schemaA");
            var schemaC2 = CreateXmlSchema("C", string.Empty);
            var schemaD1 = CreateXmlSchema("D", string.Empty);
            var schemaD2 = CreateXmlSchema("D", "http://test/schemaD");

            var wsdlA = new ServiceDescription();
            wsdlA.Types.Schemas.Add(schemaA1);
            wsdlA.Types.Schemas.Add(schemaC1);
            wsdlDocuments.Add(wsdlA);

            var wsdlB = new ServiceDescription();
            wsdlB.Types.Schemas.Add(schemaB1);
            wsdlB.Types.Schemas.Add(schemaC2);
            wsdlDocuments.Add(wsdlB);

            var schemaSet = new XmlSchemaSet();
            schemaSet.Add(schemaB2);
            schemaSet.Add(schemaD2);
            schemaSet.Add(schemaD1);
            schemaSet.Add(schemaA2);

            var actual = wsdlDocuments.MergeSchemas(schemaSet);

            Assert.AreEqual(4, actual.Count);
            Assert.IsTrue(actual.Contains(schemaA1));
            Assert.IsTrue(actual.Contains(schemaB1));
            Assert.IsTrue(actual.Contains(schemaC1));
            Assert.IsTrue(actual.Contains(schemaD2));
        }

        private static XmlSchema CreateXmlSchema(string targetNamespace, string sourceUri)
        {
            var xmlSchema = new XmlSchema
                {
                    TargetNamespace = targetNamespace,
                    SourceUri = sourceUri
                };
            return xmlSchema;
        }
    }


}
