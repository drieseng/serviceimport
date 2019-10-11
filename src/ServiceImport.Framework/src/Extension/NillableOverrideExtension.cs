using System.CodeDom;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Schema;
using ServiceImport.Framework.Helper;
using ServiceImport.Framework.Model;

namespace ServiceImport.Framework.Extension
{
    /// <summary>
    /// Allows fine-grained control over the <see cref="Element.IsNillable"/> characteristic of an
    /// <see cref="Element"/>.
    /// </summary>
    /// <remarks>
    /// This characteristic influences whether the corresponding property is nullable.
    /// </remarks>
    public class NillableOverrideExtension : IWsdlImportExtension, IContractBehavior, IXsdImportExtension
    {
        private readonly Dictionary<XmlQualifiedName, Dictionary<string, NillableOverride>> _nillableOverrides;

        public NillableOverrideExtension(Dictionary<XmlQualifiedName, Dictionary<string, NillableOverride>> nillableOverrides)
        {
            _nillableOverrides = nillableOverrides;
        }

        #region IWsdlImportExtension implementation

        void IWsdlImportExtension.BeforeImport(ServiceDescriptionCollection wsdlDocuments, XmlSchemaSet xmlSchemas, ICollection<XmlElement> policy)
        {
            OverrideNillableForElements(xmlSchemas);
        }

        void IWsdlImportExtension.ImportContract(WsdlImporter importer, WsdlContractConversionContext context)
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

        #region IXsdImportExtension implementation

        void IXsdImportExtension.BeforeImport(XmlSchemaSet xmlSchemas)
        {
            OverrideNillableForElements(xmlSchemas);
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

        private void OverrideNillableForElements(XmlSchemaSet xmlSchemas)
        {
            if (_nillableOverrides.Count == 0)
            {
                return;
            }

            foreach (XmlSchema schema in xmlSchemas.Schemas())
            {
                foreach (var complexType in schema.ComplexTypes())
                {
                    if (!_nillableOverrides.TryGetValue(complexType.QualifiedName, out var overridesForComplexType))
                    {
                        continue;
                    }

                    foreach (var element in complexType.GetSequenceElements())
                    {
                        if (!overridesForComplexType.TryGetValue(element.Name, out var nillableOverride))
                        {
                            continue;
                        }

                        element.IsNillable = nillableOverride.IsNillable;
                    }
                }
            }
        }
    }
}
