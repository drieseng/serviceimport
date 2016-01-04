using System.CodeDom;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel.Description;
using System.Xml.Schema;

namespace BRail.Nis.ServiceImport.Framework.Factory
{
    public interface IWsdlImporterFactory
    {
        WsdlImporter Create(MetadataSet metadataSet, XsdDataContractImporter xsdDataContractImporter, IDictionary<XmlTypeCode, CodeTypeReference> xmlTypeMappings);
    }
}
