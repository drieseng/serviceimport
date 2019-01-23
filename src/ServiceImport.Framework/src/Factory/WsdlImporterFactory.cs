using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel.Description;
using System.Xml;
using System.Xml.Schema;
using ServiceImport.Framework.Extension;
using ServiceImport.Framework.Model;

namespace ServiceImport.Framework.Factory
{
    public class WsdlImporterFactory : IWsdlImporterFactory
    {
        public WsdlImporter Create(MetadataSet metadataSet,
            XsdDataContractImporter xsdDataContractImporter,
            Dictionary<XmlQualifiedName, Dictionary<string, NillableOverride>> nillableOverrides,
            IDictionary<XmlTypeCode, XmlTypeMapping> xmlTypeMappings,
            DataContractGenerationOptions dataContractGenerationOptions)
        {
            var serviceModel = new ServiceModel();

            var wsdlImporter = new WsdlImporter(metadataSet);
            wsdlImporter.WsdlImportExtensions.Add(new RemoveIExtensibleDataObjectImplementationExtension());
            wsdlImporter.WsdlImportExtensions.Add(new ServiceModelBuilderExtension(serviceModel));
            wsdlImporter.WsdlImportExtensions.Add(new OperationParameterNillableExtension(serviceModel));
            wsdlImporter.WsdlImportExtensions.Add(new ComplexTypeElementTypeMappingExtension(serviceModel, xmlTypeMappings));

            if ((dataContractGenerationOptions & DataContractGenerationOptions.OptionalElementAsNullable) != 0)
            {
                wsdlImporter.WsdlImportExtensions.Add(new ComplexTypeOptionalElementsNillableExtension(serviceModel));
            }

            wsdlImporter.WsdlImportExtensions.Add(new NillableOverrideExtension(serviceModel, nillableOverrides));
            wsdlImporter.WsdlImportExtensions.Add(new EmitDefaultValueExtension(serviceModel, xmlTypeMappings));
            wsdlImporter.WsdlImportExtensions.Add(new AbstractTypeExtension(serviceModel));
            wsdlImporter.WsdlImportExtensions.Add(new OperationParameterTypeMappingExtension(xmlTypeMappings));
            wsdlImporter.WsdlImportExtensions.Add(new PascalCaseFieldNamesExtension());
            wsdlImporter.WsdlImportExtensions.Add(new OperationParameterPascalCaseExtension());
            wsdlImporter.WsdlImportExtensions.Add(new OperationContractReplyActionRemovalExtension());
            wsdlImporter.WsdlImportExtensions.Add(new XmlCommentExtension(serviceModel));
            wsdlImporter.State.Add(typeof(XsdDataContractImporter), xsdDataContractImporter);
            return wsdlImporter;
        }
    }
}
