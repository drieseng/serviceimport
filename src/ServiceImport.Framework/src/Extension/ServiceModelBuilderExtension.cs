using System;
using System.Collections.Generic;
using System.ServiceModel.Description;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Schema;
using SWSServiceDecription = System.Web.Services.Description.ServiceDescription;
using System.Collections;
using BRail.Nis.ServiceImport.Framework.Model;
using BRail.Nis.ServiceImport.Framework.Helper;

namespace BRail.Nis.ServiceImport.Framework.Extension
{
    class ServiceModelBuilderExtension : IWsdlImportExtension
    {
        private readonly ServiceModel _serviceModel;

        public ServiceModelBuilderExtension(ServiceModel serviceModel)
        {
            _serviceModel = serviceModel;
        }

        public void BeforeImport(ServiceDescriptionCollection wsdlDocuments, XmlSchemaSet xmlSchemas, ICollection<XmlElement> policy)
        {
            // unite both inline (in WSDLs) and standalone schemas
            _serviceModel.XmlSchemas = wsdlDocuments.MergeSchemas(xmlSchemas);

            var operationMessages = GetOperationMessages(wsdlDocuments);
            operationMessages.ForEach(o => _serviceModel.OperationMessages.Add(o.QualifiedName, o));

            var complexTypes = GetComplexTypes(_serviceModel.XmlSchemas);
            complexTypes.ForEach(c => _serviceModel.ComplexTypes.Add(c));
        }

        public void ImportContract(WsdlImporter importer, WsdlContractConversionContext context)
        {
        }

        public void ImportEndpoint(WsdlImporter importer, WsdlEndpointConversionContext context)
        {
        }

        private IEnumerable<ComplexType> GetComplexTypes(XmlSchemaSet xmlSchemas)
        {
            foreach (DictionaryEntry globalTypeEntry in xmlSchemas.GlobalTypes)
            {
                var globalType = globalTypeEntry.Value;

                var complexType = globalType as XmlSchemaComplexType;
                if (complexType == null || complexType.Name == null)
                    continue;

                var elements = new Dictionary<string, Element>();

                foreach (var element in complexType.GetSequenceElements())
                {
                    var schemaType = element.ElementSchemaType;
                    elements.Add(element.Name, new Element(element.Name, schemaType, element.MinOccurs, element.MaxOccurs));
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
                                    throw new Exception(string.Format("Wrapper element '{0}' could not be found.", parametersPart.Element));

                                foreach (var parameterElement in wrapperElement.GetWrapperElementParameters())
                                    operationParameters.Add(parameterElement.Name, new OperationParameterInfo(parameterElement));

                                yield return new OperationMessageInfo(operationMessage.Message, operationParameters);
                            }
                        }
                    }
                }
            }
        }
    }
}
