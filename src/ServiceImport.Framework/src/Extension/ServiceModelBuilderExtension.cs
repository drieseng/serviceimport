using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel.Description;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Schema;
using ServiceImport.Framework.Helper;
using ServiceImport.Framework.Model;
using SWSServiceDecription = System.Web.Services.Description.ServiceDescription;

namespace ServiceImport.Framework.Extension
{
    class ServiceModelBuilderExtension : IWsdlImportExtension, IXsdImportExtension
    {
        private readonly ServiceModel _serviceModel;

        public ServiceModelBuilderExtension(ServiceModel serviceModel)
        {
            _serviceModel = serviceModel;
        }

        #region IWsdlImportExtension implementation

        void IWsdlImportExtension.BeforeImport(ServiceDescriptionCollection wsdlDocuments, XmlSchemaSet xmlSchemas, ICollection<XmlElement> policy)
        {
            // unite both inline (in WSDLs) and standalone schemas
            _serviceModel.XmlSchemas = wsdlDocuments.MergeSchemas(xmlSchemas);

            var operationMessages = GetOperationMessages(wsdlDocuments);
            operationMessages.ForEach(o => _serviceModel.OperationMessages.Add(o.QualifiedName, o));

            var complexTypes = GetComplexTypes(_serviceModel.XmlSchemas);
            complexTypes.ForEach(c => _serviceModel.ComplexTypes.Add(c));
        }

        void IWsdlImportExtension.ImportContract(WsdlImporter importer, WsdlContractConversionContext context)
        {
        }

        void IWsdlImportExtension.ImportEndpoint(WsdlImporter importer, WsdlEndpointConversionContext context)
        {
        }

        #endregion IWsdlImportExtension implementation

        #region IXsdImportExtension implementation

        void IXsdImportExtension.BeforeImport(XmlSchemaSet xmlSchemas)
        {
            _serviceModel.XmlSchemas = xmlSchemas;
            var complexTypes = GetComplexTypes(_serviceModel.XmlSchemas);
            complexTypes.ForEach(c => _serviceModel.ComplexTypes.Add(c));
        }

        void IXsdImportExtension.ImportContract(XsdDataContractImporter importer)
        {
        }

        #endregion IXsdImportExtension implementation

        #region IDataContractGenerationExtension implementation

        void IDataContractGenerationExtension.GenerateContract(CodeCompileUnit compileUnit)
        {
        }

        #endregion IDataContractGenerationExtension implementation

        private static IEnumerable<ComplexType> GetComplexTypes(XmlSchemaSet xmlSchemas)
        {
            foreach (DictionaryEntry globalTypeEntry in xmlSchemas.GlobalTypes)
            {
                var globalType = globalTypeEntry.Value;

                var complexType = globalType as XmlSchemaComplexType;
                if (complexType == null || complexType.Name == null)
                    continue;

                var elements = new List<Element>();

                foreach (var element in complexType.GetSequenceElements())
                {
                    //element.SchemaTypeName = new XmlQualifiedName("guid", "http://schemas.microsoft.com/2003/10/Serialization/");

                    elements.Add(new Element(element));
                }

                yield return new ComplexType(complexType.QualifiedName, complexType.IsAbstract, elements);
            }
        }

        private IEnumerable<OperationMessageInfo> GetOperationMessages(ServiceDescriptionCollection wsdlDocuments)
        {
            foreach (SWSServiceDecription wsdl in wsdlDocuments)
            {
                foreach (PortType portType in wsdl.PortTypes)
                {
                    foreach (Operation operation in portType.Operations)
                    {
                        foreach (OperationMessage operationMessage in operation.Messages)
                        {
                            foreach (Message message in wsdl.Messages)
                            {
                                if (message.Name != operationMessage.Message.Name)
                                    continue;

                                var parametersPart = message.Parts.SingleOrDefault<MessagePart>(p => p.Name == "parameters");
                                if (parametersPart == null)
                                    continue;

                                var operationParameters = new Dictionary<string, OperationParameterInfo>();

                                var wrapperElement = _serviceModel.XmlSchemas.GlobalElements[parametersPart.Element] as XmlSchemaElement;
                                if (wrapperElement == null)
                                    throw new Exception($"Wrapper element '{parametersPart.Element}' could not be found.");

                                foreach (var parameterElement in wrapperElement.GetWrapperElementParameters())
                                {
                                    operationParameters.Add(parameterElement.Name, new OperationParameterInfo(parameterElement));
                                }

                                yield return new OperationMessageInfo(operationMessage.Message, operationParameters);
                            }
                        }
                    }
                }
            }
        }
    }
}
