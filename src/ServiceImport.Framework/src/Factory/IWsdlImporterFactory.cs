using System.CodeDom;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel.Description;
using System.Xml;
using System.Xml.Schema;
using ServiceImport.Framework.Model;

namespace ServiceImport.Framework.Factory
{
    public interface IWsdlImporterFactory
    {
        WsdlImporter Create(MetadataSet metadataSet,
            XsdDataContractImporter xsdDataContractImporter,
            Dictionary<XmlQualifiedName, Dictionary<string, NillableOverride>> nillableOverrides,
            IDictionary<XmlTypeCode, XmlTypeMapping> xmlTypeMappings,
            DataContractGenerationOptions dataContractGenerationOptions);
    }
}
