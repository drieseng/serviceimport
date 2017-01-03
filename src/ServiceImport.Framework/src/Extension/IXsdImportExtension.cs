using System.Runtime.Serialization;
using System.Xml.Schema;

namespace BRail.Nis.ServiceImport.Framework.Extension
{
    public interface IXsdImportExtension : IDataContractGenerationExtension
    {
        /// <summary>
        /// Called prior to importing metadata documents.
        /// </summary>
        /// <param name="xmlSchemas">The schema collection to import.</param>
        void BeforeImport(XmlSchemaSet xmlSchemas);

        /// <summary>
        /// Called when importing a contract.
        /// </summary>
        /// <param name="importer">The importer.</param>
        void ImportContract(XsdDataContractImporter importer);
    }
}
