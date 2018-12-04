using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Schema;
using ServiceImport.Framework.Model;

namespace ServiceImport.Framework.Extension
{
    /// <summary>
    /// Marks optional elements of complex types as nillable before the import.
    /// </summary>
    /// <remarks>
    /// This is done to ensure the types of corresponding properties in the generated code are nullable in case
    /// of value types.
    /// </remarks>
    public class ComplexTypeOptionalElementsNillableExtension : IWsdlImportExtension, IContractBehavior, IXsdImportExtension
    {
        private const string SerializationArraysNamespace = "http://schemas.microsoft.com/2003/10/Serialization/Arrays";

        private readonly ServiceModel _serviceModel;

        public ComplexTypeOptionalElementsNillableExtension(ServiceModel serviceModel)
        {
            _serviceModel = serviceModel;
        }

        #region IWsdlImportExtension implementation

        void IWsdlImportExtension.BeforeImport(ServiceDescriptionCollection wsdlDocuments, XmlSchemaSet xmlSchemas, ICollection<XmlElement> policy)
        {
            MarkOptionalElementsNillable();
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
            MarkOptionalElementsNillable();
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

        private void MarkOptionalElementsNillable()
        {
            foreach (var complexType in _serviceModel.ComplexTypes)
            {
                // do not touch the IsNillable for the elements in specialized array serialization complex types,
                // if not the XsdDataContractImporter will generate wrapper classes for these arrays
                if (complexType.QualifiedName.Namespace == SerializationArraysNamespace)
                    continue;

                foreach (var element in complexType.Elements)
                {
                    if (element.MinOccurs == 0)
                    {
                        element.IsNillable = true;
                    }
                }
            }
        }
    }
}
