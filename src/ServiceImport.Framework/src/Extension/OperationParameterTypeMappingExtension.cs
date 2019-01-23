using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Schema;
using ServiceImport.Framework.CodeDom;
using ServiceImport.Framework.Helper;
using ServiceImport.Framework.Model;
using SWSServiceDescription = System.Web.Services.Description.ServiceDescription;
using SWSMessage = System.Web.Services.Description.Message;

namespace ServiceImport.Framework.Extension
{
    public class OperationParameterTypeMappingExtension : IOperationContractGenerationExtension, IOperationBehavior, IWsdlImportExtension
    {
        private ServiceDescriptionCollection _wsdlDocuments;
        private readonly IDictionary<XmlTypeCode, XmlTypeMapping> _xmlTypeMapping;

        public OperationParameterTypeMappingExtension(IDictionary<XmlTypeCode, XmlTypeMapping> xmlTypeMapping)
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
                {
                    continue;
                }

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
            if (portType == null)
            {
                throw new Exception($"Port type '{context.Contract.Contract.Namespace}:{context.Contract.Contract.Name}' not found.");
            }

            var operation = FindOperationByName(portType.Operations, context.Operation.Name);
            if (operation == null)
            {
                throw new Exception($"Operation '{context.Operation.Name} not found in port type '{portType.ServiceDescription.TargetNamespace}:{portType.ServiceDescription.Name}'.");
            }

            foreach (var message in context.Operation.Messages)
            {
                var wrapperElement = FindSchemaElementByQualifiedName(portType.ServiceDescription.Types.Schemas, new XmlQualifiedName(message.Body.WrapperName, message.Body.WrapperNamespace));
                if (wrapperElement == null)
                {
                    throw new Exception($"Schema element '{message.Body.WrapperNamespace}:{message.Body.WrapperName}' not found.");
                }

                var parameterRegister = CreateParameterRegister(wrapperElement);

                foreach (var part in message.Body.Parts)
                {
                    if (!parameterRegister.TryGetValue(part.Name, out var parameterElement))
                    {
                        throw new Exception();
                    }

                    if (!_xmlTypeMapping.TryGetValue(parameterElement.ElementSchemaType.TypeCode, out var typeMapping))
                        continue;

                    var methodParameter = FindMethodParameterByName(context.SyncMethod, part.Name);
                    if (methodParameter == null)
                        throw new Exception();

                    if (parameterElement.MinOccurs == 0 && (typeMapping.IsStruct || typeMapping.IsEnum))
                    {
                        methodParameter.Type = typeMapping.CodeTypeReference.ToNullable();
                    }
                    else
                    {
                        methodParameter.Type = typeMapping.CodeTypeReference;
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

        private OperationMessage FindOperationMessageByName(OperationMessageCollection operationMessages, string wrapperName, string wrapperNamespace)
        {
            foreach (OperationMessage operationMessage in operationMessages)
            {
                var message = operationMessage.Message;
                if (message.Name == wrapperName && message.Namespace == wrapperNamespace)
                    return operationMessage;
            }

            return null;
        }

        private static Operation FindOperationByName(OperationCollection operations, string operationName)
        {
            Operation found = null;

            foreach (Operation operation in operations)
            {
                if (operation.Name == operationName)
                {
                    if (found == null)
                    {
                        found = operation;
                    }
                    else
                    {
                        throw new Exception($"More than one operation with name '{operationName}'.");
                    }
                }
            }

            return found;

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
