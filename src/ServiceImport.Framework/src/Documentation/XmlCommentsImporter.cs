using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Description;
using System.Web.Services.Description;
using System.Xml.Schema;

namespace ServiceImport.Framework.Documentation
{
    public class XmlCommentsImporter : IServiceBehavior, IWsdlImportExtension
    {
        internal static ImportOptions Options = new ImportOptions();
        internal ServiceDescriptionCollection WsdlDocuments;
        internal XmlSchemaSet XmlSchemas;

        void IWsdlImportExtension.BeforeImport(ServiceDescriptionCollection wsdlDocuments,
                                               XmlSchemaSet xmlSchemas,
                                               ICollection<System.Xml.XmlElement> policy)
        {
            this.WsdlDocuments = wsdlDocuments;
            this.XmlSchemas = xmlSchemas;
        }

        internal static void AddXmlComment(CodeTypeMember member, string documentation, ImportOptions options)
        {
            var commentLines = XmlCommentsUtils.ParseAndReformatComment(documentation, options.Format, options.WrapLongLines);
            var commentStatements = commentLines.Select(s => new CodeCommentStatement(s, true)).ToArray();
            member.Comments.AddRange(commentStatements);
        }

        private static string GetDocumentation(DocumentableItem item)
        {
            if (item.DocumentationElement != null)
                return item.DocumentationElement.InnerText;
            return item.Documentation;
        }

        void IWsdlImportExtension.ImportContract(WsdlImporter importer, WsdlContractConversionContext context)
        {
            var documentation = GetDocumentation(context.WsdlPortType);
            context.Contract.Behaviors.Add(new XmlCommentsSvcExtension(this, documentation));

            foreach (Operation operation in context.WsdlPortType.Operations)
            {
                documentation = GetDocumentation(operation);
                var operationDescription = context.Contract.Operations.Find(operation.Name);
                if (!string.IsNullOrEmpty(documentation))
                {
                    operationDescription.Behaviors.Add(new XmlCommentsOpExtension(this, documentation));
                }
            }
        }

        void IWsdlImportExtension.ImportEndpoint(WsdlImporter importer, WsdlEndpointConversionContext context)
        {
        }

        void IServiceBehavior.AddBindingParameters(System.ServiceModel.Description.ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase, System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
        }

        void IServiceBehavior.ApplyDispatchBehavior(System.ServiceModel.Description.ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase)
        {
        }

        void IServiceBehavior.Validate(System.ServiceModel.Description.ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase)
        {
        }
    }

    public class XmlCommentsSvcExtension : IContractBehavior, IServiceContractGenerationExtension
    {
        #region IContractBehavior
        void IContractBehavior.AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters) { }
        void IContractBehavior.ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.ClientRuntime clientRuntime) { }
        void IContractBehavior.ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.DispatchRuntime dispatchRuntime) { }
        void IContractBehavior.Validate(ContractDescription contractDescription, ServiceEndpoint endpoint) { }
        #endregion

        private readonly string _documentation;
        private readonly XmlCommentsImporter _importer;

        public XmlCommentsSvcExtension(XmlCommentsImporter importer, string documentation)
        {
            this._documentation = documentation;
            this._importer = importer;
        }

        void IServiceContractGenerationExtension.GenerateContract(ServiceContractGenerationContext context)
        {
            if (!String.IsNullOrEmpty(_documentation))
                XmlCommentsImporter.AddXmlComment(context.ContractType, _documentation, XmlCommentsImporter.Options);
            AddXmlCommentsToDataContracts(context);
        }

        private void AddXmlCommentsToDataContracts(ServiceContractGenerationContext context)
        {
            var codeMembers = CodeDomUtils.EnumerateCodeMembers(context.ServiceContractGenerator.TargetCompileUnit);
            var documentedItems = new Dictionary<string, string>();

            WsdlUtils.EnumerateDocumentedItems(_importer.WsdlDocuments, documentedItems);
            WsdlUtils.EnumerateDocumentedItems(_importer.XmlSchemas, documentedItems);

            foreach (var documentedItem in documentedItems)
            {
                if (codeMembers.TryGetValue(documentedItem.Key, out var codeMember))
                {
                    XmlCommentsImporter.AddXmlComment(codeMember, documentedItem.Value, XmlCommentsImporter.Options);
                }
            }
        }
    }

    public class XmlCommentsOpExtension : IOperationBehavior, IOperationContractGenerationExtension
    {
        #region IOperationBehavior
        void IOperationBehavior.AddBindingParameters(OperationDescription operationDescription, System.ServiceModel.Channels.BindingParameterCollection bindingParameters) { }
        void IOperationBehavior.ApplyClientBehavior(OperationDescription operationDescription, System.ServiceModel.Dispatcher.ClientOperation clientOperation) { }
        void IOperationBehavior.ApplyDispatchBehavior(OperationDescription operationDescription, System.ServiceModel.Dispatcher.DispatchOperation dispatchOperation) { }
        void IOperationBehavior.Validate(OperationDescription operationDescription) { }
        #endregion

        private readonly string _documentation;
        private XmlCommentsImporter _importer;

        public XmlCommentsOpExtension(XmlCommentsImporter importer, string documentation)
        {
            this._documentation = documentation;
            this._importer = importer;
        }

        void IOperationContractGenerationExtension.GenerateOperation(OperationContractGenerationContext context)
        {
            XmlCommentsImporter.AddXmlComment(context.SyncMethod, _documentation, XmlCommentsImporter.Options);
        }
    }
}