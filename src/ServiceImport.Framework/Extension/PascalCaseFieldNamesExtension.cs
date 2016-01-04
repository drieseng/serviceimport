using BRail.Nis.ServiceImport.Framework.CodeDom;
using System.CodeDom;
using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Schema;

namespace BRail.Nis.ServiceImport.Framework.Extension
{
    public class PascalCaseFieldNamesExtension : IWsdlImportExtension, IContractBehavior, IServiceContractGenerationExtension
    {
        #region IWsdlImportExtension implementation

        void IWsdlImportExtension.BeforeImport(ServiceDescriptionCollection wsdlDocuments, XmlSchemaSet xmlSchemas, ICollection<XmlElement> policy)
        {
        }

        public void ImportContract(WsdlImporter importer, WsdlContractConversionContext context)
        {
            context.Contract.ContractBehaviors.Add(this);
        }

        void IWsdlImportExtension.ImportEndpoint(WsdlImporter importer, WsdlEndpointConversionContext context)
        {
        }

        #endregion IWsdlImportExtension implementation

        #region IContractBehavior implementation

        void IContractBehavior.Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {
        }

        void IContractBehavior.ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, DispatchRuntime dispatchRuntime)
        {
        }

        void IContractBehavior.ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        void IContractBehavior.AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        #endregion IContractBehavior implementation

        #region IServiceContractGenerationExtension implementation

        public void GenerateContract(ServiceContractGenerationContext context)
        {
            // use the compile unit of the ServiceContractGenerator to have types defined
            // in XSDs and WSDLs
            var compileUnit = context.ServiceContractGenerator.TargetCompileUnit;

            foreach (var typeDeclaration in compileUnit.Types())
                PascalCaseTypeMembers(typeDeclaration);
        }

        private static void PascalCaseTypeMembers(CodeTypeDeclaration typeDeclaration)
        {
            foreach (var member in typeDeclaration.Members)
            {
                var field = member as CodeMemberField;
                if (field != null)
                {
                    field.Name = field.Name.ToPascalCase();
                    continue;
                }

                var property = member as CodeMemberProperty;
                if (property != null)
                {
                    if (property.GetStatements.Count == 1)
                    {
                        var returnStatement = property.GetStatements[0] as CodeMethodReturnStatement;
                        if (returnStatement != null && returnStatement.Expression != null)
                        {
                            var fieldReference = returnStatement.Expression as CodeFieldReferenceExpression;
                            if (fieldReference != null)
                            {
                                fieldReference.FieldName = fieldReference.FieldName.ToPascalCase();
                            }
                        }
                    }

                    if (property.SetStatements.Count == 1)
                    {
                        var assignStatement = property.SetStatements[0] as CodeAssignStatement;
                        if (assignStatement != null && assignStatement.Left != null)
                        {
                            var left = assignStatement.Left as CodeFieldReferenceExpression;
                            if (left != null)
                            {
                                left.FieldName = left.FieldName.ToPascalCase();
                            }
                        }
                    }

                    continue;
                }

                var type = member as CodeTypeDeclaration;
                if (type != null)
                {
                    PascalCaseTypeMembers(type);
                    continue;
                }
            }
        }

        #endregion IServiceContractGenerationExtension implementation
    }
}
