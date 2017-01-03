using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Web.Services.Description;
using SWSServiceDescription = System.Web.Services.Description.ServiceDescription;
using SWSMessage = System.Web.Services.Description.Message;
using System.Xml.Schema;
using System.Collections;
using System.Xml;
using BRail.Nis.ServiceImport.Framework.CodeDom;
using BRail.Nis.ServiceImport.Framework.Helper;

namespace BRail.Nis.ServiceImport.Framework.Extension
{
    public class OperationParameterTypeMappingExtension : IOperationContractGenerationExtension, IOperationBehavior, IWsdlImportExtension
    {
        private ServiceDescriptionCollection _wsdlDocuments;
        private readonly IDictionary<XmlTypeCode, CodeTypeReference> _xmlTypeMapping;

        public OperationParameterTypeMappingExtension(IDictionary<XmlTypeCode, CodeTypeReference> xmlTypeMapping)
        {
            _xmlTypeMapping = xmlTypeMapping;
        }

        #region IWsdlImportExtension implementation

        public void BeforeImport(ServiceDescriptionCollection wsdlDocuments, XmlSchemaSet xmlSchemas, ICollection<XmlElement> policy)
        {
            _wsdlDocuments = wsdlDocuments;
        }

        public void ImportContract(WsdlImporter importer, WsdlContractConversionContext context)
        {
            foreach (Operation operation in context.WsdlPortType.Operations)
            {
                var description = context.Contract.Operations.Find(operation.Name);
                if (description == null)
                    continue;

                description.OperationBehaviors.Add(this);
            }
        }

        public void ImportEndpoint(WsdlImporter importer, WsdlEndpointConversionContext context)
        {
        }

        #endregion IWsdlImportExtension implementation

        #region IOperationBehavior implementation

        void IOperationBehavior.AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {
        }

        void IOperationBehavior.ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
        }

        void IOperationBehavior.ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
        }

        void IOperationBehavior.Validate(OperationDescription operationDescription)
        {
        }

        #endregion IOperationBehavior implementation

        #region IOperationContractGenerationExtension implementation

        public void GenerateOperation(OperationContractGenerationContext context)
        {
            var portType = FindPortType(_wsdlDocuments, context.Contract.Contract.Name, context.Contract.Contract.Namespace);

            var operation = FindOperationByName(portType.Operations, context.Operation.Name);

            foreach (var message in context.Operation.Messages)
            {
                var operationMessage = FindOperationMessageByName(operation.Messages, message.Body.WrapperName);
                if (operationMessage == null)
                    break;

                var wsdlMessage = FindWsdlMessage(portType.ServiceDescription.Messages, operationMessage.Message.Name);
                var wrapperElement = FindSchemaElementByQualifiedName(portType.ServiceDescription.Types.Schemas, wsdlMessage.Parts[0].Element);
                var parameterRegister = CreateParameterRegister(wrapperElement);

                if (message.Direction == MessageDirection.Input)
                {
                    foreach (var part in message.Body.Parts)
                    {
                        XmlSchemaElement parameterElement;
                        if (!parameterRegister.TryGetValue(part.Name, out parameterElement))
                        {
                            throw new Exception();
                        }

                        CodeTypeReference typeReference;
                        if (!_xmlTypeMapping.TryGetValue(parameterElement.ElementSchemaType.TypeCode, out typeReference))
                            continue;

                        var methodParameter = FindMethodParameterByName(context.SyncMethod, part.Name);
                        if (methodParameter == null)
                            throw new Exception();

                        if (parameterElement.MinOccurs == 0)
                        {
                            methodParameter.Type = typeReference.ToNullable();
                        }
                        else
                        {
                            methodParameter.Type = typeReference;
                        }
                    }
                }
            }
        }

        #endregion IOperationContractGenerationExtension implementation

        private static IDictionary<string, XmlSchemaElement> CreateParameterRegister(XmlSchemaElement element)
        {
            var parameterRegister = new Dictionary<string, XmlSchemaElement>();

            foreach (var parameterElement in element.GetWrapperElementParameters())
                    parameterRegister.Add(parameterElement.Name, parameterElement);

            return parameterRegister;
        }

        private SWSMessage FindWsdlMessage(MessageCollection messages, string name)
        {
            foreach (SWSMessage message in messages)
            {
                if (message.Name == name)
                    return message;
            }

            return null;
        }

        private XmlSchemaElement FindSchemaElementByQualifiedName(ICollection schemas, XmlQualifiedName name)
        {
            foreach (XmlSchema schema in schemas)
            {
                if (schema.TargetNamespace != name.Namespace)
                    continue;

                var element = schema.Elements[name];
                if (element != null)
                    return (XmlSchemaElement) element;
            }

            return null;
        }

        private CodeParameterDeclarationExpression FindMethodParameterByName(CodeMemberMethod method, string parameterName)
        {
            foreach (CodeParameterDeclarationExpression parameter in method.Parameters)
            {
                if (parameter.Name == parameterName)
                    return parameter;
            }

            return null;
        }

        private OperationMessage FindOperationMessageByName(OperationMessageCollection operationMessages, string wrapperName)
        {
            foreach (OperationMessage operationMessage in operationMessages)
            {
                var message = operationMessage.Message;
                if (message.Name == wrapperName)
                    return operationMessage;
            }

            return null;
        }

        private static Operation FindOperationByName(OperationCollection operations, string operationName)
        {
            foreach (Operation operation in operations)
            {
                if (operation.Name == operationName)
                    return operation;
            }

            return null;
        }

        private static PortType FindPortType(ServiceDescriptionCollection wsdlDocuments, string name, string @namespace)
        {
            foreach (SWSServiceDescription serviceDescription in wsdlDocuments)
            {
                if (serviceDescription.TargetNamespace != @namespace)
                    continue;

                foreach (PortType portType in serviceDescription.PortTypes)
                {
                    if (portType.Name == name)
                        return portType;
                }
            }

            return null;
        }
    }
}
