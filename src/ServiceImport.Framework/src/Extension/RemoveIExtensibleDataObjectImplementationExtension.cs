using System.CodeDom;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Schema;
using ServiceImport.Framework.CodeDom;

namespace ServiceImport.Framework.Extension
{
    public class RemoveIExtensibleDataObjectImplementationExtension : IServiceContractGenerationExtension, IWsdlImportExtension, IContractBehavior, IXsdImportExtension
    {
        #region IWsdlImportExtension implementation

        public void BeforeImport(ServiceDescriptionCollection wsdlDocuments, XmlSchemaSet xmlSchemas, ICollection<XmlElement> policy)
        {
        }

        public void ImportContract(WsdlImporter importer, WsdlContractConversionContext context)
        {
            context.Contract.Behaviors.Add(this);
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

        #region  implementation

        void IServiceContractGenerationExtension.GenerateContract(ServiceContractGenerationContext context)
        {
            // use the compile unit of the ServiceContractGenerator to have types defined
            // in XSDs and WSDLs
            var compileUnit = context.ServiceContractGenerator.TargetCompileUnit;
            foreach (var typeDeclaration in compileUnit.Types())
                RemoveIExtensibleDataObjectImplementation(typeDeclaration);
        }

        #endregion IServiceContractGenerationExtension implementation

        #region IXsdImportExtension implementation

        void IXsdImportExtension.BeforeImport(XmlSchemaSet xmlSchemas)
        {
        }

        void IXsdImportExtension.ImportContract(XsdDataContractImporter importer)
        {
        }

        void IDataContractGenerationExtension.GenerateContract(CodeCompileUnit compileUnit)
        {
            foreach (var typeDeclaration in compileUnit.Types())
                RemoveIExtensibleDataObjectImplementation(typeDeclaration);
        }

        #endregion IXsdImportExtension implementation

        private static void RemoveIExtensibleDataObjectImplementation(CodeTypeDeclaration typeDeclaration)
        {
            var implementsIExtensibleDataObject = false;
            var baseTypeCount = typeDeclaration.BaseTypes.Count;

            for (var i = (baseTypeCount - 1); i >= 0; i--)
            {
                var baseTypeReference = typeDeclaration.BaseTypes[i];

                if (baseTypeReference.BaseType == typeof(IExtensibleDataObject).FullName)
                {
                    implementsIExtensibleDataObject = true;
                    typeDeclaration.BaseTypes.RemoveAt(i);
                    break;
                }
            }

            var memberCount = typeDeclaration.Members.Count;
            for (var j = (memberCount - 1); j >= 0; j--)
            {
                var member = typeDeclaration.Members[j];

                if (implementsIExtensibleDataObject)
                {
                    if (member is CodeMemberProperty property)
                    {
                        if (property.Name == "ExtensionData")
                            typeDeclaration.Members.RemoveAt(j);
                        continue;
                    }

                    if (member is CodeMemberField field)
                    {
                        if (field.Name == "extensionDataField")
                            typeDeclaration.Members.RemoveAt(j);
                        continue;
                    }
                }

                if (member is CodeTypeDeclaration type)
                    RemoveIExtensibleDataObjectImplementation(type);
            }
        }
    }
}
