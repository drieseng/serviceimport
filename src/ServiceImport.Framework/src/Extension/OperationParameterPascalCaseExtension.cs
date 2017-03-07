using System.CodeDom;
using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Schema;
using ServiceImport.Framework.CodeDom;

namespace ServiceImport.Framework.Extension
{
    public class OperationParameterPascalCaseExtension : IOperationContractGenerationExtension, IWsdlImportExtension, IOperationBehavior
    {
        #region IWsdlImportExtension implementation

        public void BeforeImport(ServiceDescriptionCollection wsdlDocuments, XmlSchemaSet xmlSchemas, ICollection<XmlElement> policy)
        {
        }

        public void ImportContract(WsdlImporter importer, WsdlContractConversionContext context)
        {
            foreach (Operation operation in context.WsdlPortType.Operations)
            {
                var operationDescription = context.GetOperationDescription(operation);
                operationDescription.OperationBehaviors.Add(this);
            }
        }

        public void ImportEndpoint(WsdlImporter importer, WsdlEndpointConversionContext context)
        {
        }

        #endregion IWsdlImportExtension implementation

        #region IOperationContractGenerationExtension implementation

        public void GenerateOperation(OperationContractGenerationContext context)
        {
            foreach (CodeParameterDeclarationExpression parameter in context.SyncMethod.Parameters)
            {
                var parameterName = parameter.Name;

                if (parameterName.IsPascalCase())
                    continue;

                parameter.Name = parameterName.ToPascalCase();

                var messageParameterAttribute = new CodeAttributeDeclaration(new CodeTypeReference("System.ServiceModel.MessageParameterAttribute"),
                    new CodeAttributeArgument("Name", new CodeSnippetExpression("\"" + parameterName + "\"")));
                parameter.CustomAttributes.Add(messageParameterAttribute);
            }
        }

        #endregion IOperationContractGenerationExtension implementation

        #region IOperationBehavior implementation

        void IOperationBehavior.Validate(OperationDescription operationDescription)
        {
        }

        void IOperationBehavior.ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
        }

        void IOperationBehavior.ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
        }

        void IOperationBehavior.AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {
        }

        #endregion IOperationBehavior implementation
    }
}
