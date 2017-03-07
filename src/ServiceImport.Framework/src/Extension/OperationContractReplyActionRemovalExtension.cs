using System.CodeDom;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Schema;
using ServiceImport.Framework.Helper;

namespace ServiceImport.Framework.Extension
{
    /// <summary>
    /// Removes the <see cref="OperationContractAttribute.ReplyAction"/> argument from the <see cref="OperationContractAttribute"/>
    /// of operation methods in a service contract when its value is &quot;*&quot; (an asterisk).
    /// </summary>
    /// <remarks>
    /// WCF generates an empty WSDL when <see cref="OperationContractAttribute.ReplyAction"/> is &quot;*&quot;.
    /// </remarks>
    public class OperationContractReplyActionRemovalExtension : IOperationContractGenerationExtension, IWsdlImportExtension, IOperationBehavior
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
            var operationContractAttribute = context.SyncMethod.CustomAttributes.SingleOrDefault<CodeAttributeDeclaration>(p => p.Name == typeof(OperationContractAttribute).FullName);
            if (operationContractAttribute == null)
                return;

            var replyActionArgument = operationContractAttribute.Arguments.SingleOrDefault<CodeAttributeArgument>(p => p.Name == "ReplyAction");
            if (replyActionArgument == null)
                return;

            var expression = replyActionArgument.Value as CodePrimitiveExpression;
            if (expression == null)
                return;

            var expressionText = expression.Value as string;
            if (expressionText == null)
                return;

            if (expressionText == "*")
                operationContractAttribute.Arguments.Remove(replyActionArgument);
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

