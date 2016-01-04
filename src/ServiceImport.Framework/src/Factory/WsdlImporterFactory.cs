using BRail.Nis.ServiceImport.Framework.Extension;
using BRail.Nis.ServiceImport.Framework.Model;
using System.CodeDom;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel.Description;
using System.Xml.Schema;

namespace BRail.Nis.ServiceImport.Framework.Factory
{
    public class WsdlImporterFactory : IWsdlImporterFactory
    {
        public WsdlImporter Create(MetadataSet metadataSet, XsdDataContractImporter xsdDataContractImporter, IDictionary<XmlTypeCode, CodeTypeReference> xmlTypeMappings)
        {
            var serviceModel = new ServiceModel();

            var wsdlImporter = new WsdlImporter(metadataSet);
            wsdlImporter.WsdlImportExtensions.Add(new RemoveIExtensibleDataObjectImplementationExtension());
            wsdlImporter.WsdlImportExtensions.Add(new ServiceModelBuilderExtension(serviceModel));
            wsdlImporter.WsdlImportExtensions.Add(new OperationParameterNillableExtension(serviceModel));
            wsdlImporter.WsdlImportExtensions.Add(new ComplexTypeElementTypeMappingExtension(serviceModel, xmlTypeMappings));
            wsdlImporter.WsdlImportExtensions.Add(new ComplexTypeOptionalElementsNillableExtension(serviceModel));
            wsdlImporter.WsdlImportExtensions.Add(new AbstractTypeExtension(serviceModel));
            wsdlImporter.WsdlImportExtensions.Add(new OperationParameterTypeMappingExtension(xmlTypeMappings));
            wsdlImporter.WsdlImportExtensions.Add(new PascalCaseFieldNamesExtension());
            wsdlImporter.WsdlImportExtensions.Add(new OperationParameterPascalCaseExtension());
            wsdlImporter.WsdlImportExtensions.Add(new OperationContractReplyActionRemovalExtension());
            wsdlImporter.State.Add(typeof(XsdDataContractImporter), xsdDataContractImporter);
            return wsdlImporter;
        }
    }
}
