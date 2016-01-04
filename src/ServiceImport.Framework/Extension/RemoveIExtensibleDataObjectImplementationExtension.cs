using System.CodeDom;
using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Schema;
using System.Runtime.Serialization;
using BRail.Nis.ServiceImport.Framework.CodeDom;

namespace BRail.Nis.ServiceImport.Framework.Extension
{
    public class RemoveIExtensibleDataObjectImplementationExtension : IServiceContractGenerationExtension, IWsdlImportExtension, IContractBehavior
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

        #region IServiceContractGenerationExtension implementation

        public void GenerateContract(ServiceContractGenerationContext context)
        {
            // use the compile unit of the ServiceContractGenerator to have types defined
            // in XSDs and WSDLs
            var compileUnit = context.ServiceContractGenerator.TargetCompileUnit;

            foreach (var typeDeclaration in compileUnit.Types())
                RemoveIExtensibleDataObjectImplementation(typeDeclaration);
        }

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
                    var property = member as CodeMemberProperty;
                    if (property != null)
                    {
                        if (property.Name == "ExtensionData")
                            typeDeclaration.Members.RemoveAt(j);
                        continue;
                    }

                    var field = member as CodeMemberField;
                    if (field != null)
                    {
                        if (field.Name == "extensionDataField")
                            typeDeclaration.Members.RemoveAt(j);
                        continue;
                    }
                }

                var type = member as CodeTypeDeclaration;
                if (type != null)
                {
                    RemoveIExtensibleDataObjectImplementation(type);
                    continue;
                }
            }
        }

        #endregion IServiceContractGenerationExtension implementation
    }
}
