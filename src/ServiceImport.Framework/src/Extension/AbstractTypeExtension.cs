using BRail.Nis.ServiceImport.Framework.CodeDom;
using BRail.Nis.ServiceImport.Framework.Helper;
using BRail.Nis.ServiceImport.Framework.Model;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Schema;

namespace BRail.Nis.ServiceImport.Framework.Extension
{
    /// <summary>
    /// <para>
    /// The <see cref="DataContractSerializer"/> does not support abstract complex types.
    /// </para>
    /// <para>
    /// This extension provides a workaround for this limitation by removing the abstract
    /// modifier from complex types prior to the import of metadata, and marking the generated
    /// types as abstract when the contract is generated.
    /// </para>
    /// </summary>
    /// <seealso cref="!:https://msdn.microsoft.com/en-us/library/ms733112.aspx"/>
    public class AbstractTypeExtension : IServiceContractGenerationExtension, IWsdlImportExtension, IContractBehavior, IXsdImportExtension, IDataContractGenerationExtension
    {
        private XsdDataContractImporter _xsdDataContractImporter;
        private readonly ServiceModel _serviceModel;

        public AbstractTypeExtension(ServiceModel serviceModel)
        {
            _serviceModel = serviceModel;
        }

        #region IWsdlImportExtension implementation

        void IWsdlImportExtension.BeforeImport(ServiceDescriptionCollection wsdlDocuments, XmlSchemaSet xmlSchemas, ICollection<XmlElement> policy)
        {
            UnsetIsAbstractFlagOnGlobalComplexType(_serviceModel.XmlSchemas);
        }

        public void ImportContract(WsdlImporter importer, WsdlContractConversionContext context)
        {
            _xsdDataContractImporter = importer.Get<XsdDataContractImporter>();

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
            MarkTypesAbstract(compileUnit);
        }

        #endregion IServiceContractGenerationExtension implementation

        #region IXsdImportExtension

        void IXsdImportExtension.BeforeImport(XmlSchemaSet xmlSchemaSet)
        {
        }

        /// <summary>Called when importing a contract.</summary>
        /// <param name="importer">The importer.</param>
        void IXsdImportExtension.ImportContract(XsdDataContractImporter importer)
        {
            _xsdDataContractImporter = importer;
        }

        #endregion IXsdImportExtension

        #region IDataContractGenerationExtension

        void IDataContractGenerationExtension.GenerateContract(CodeCompileUnit compileUnit)
        {
            MarkTypesAbstract(compileUnit);
        }

        #endregion IDataContractGenerationExtension

        private void UnsetIsAbstractFlagOnGlobalComplexType(XmlSchemaSet xmlSchemaSet)
        {
            foreach (DictionaryEntry globalTypeEntry in xmlSchemaSet.GlobalTypes)
            {
                var globalType = globalTypeEntry.Value;

                var complexType = globalType as XmlSchemaComplexType;
                if (complexType == null || complexType.Name == null)
                    continue;

                if (complexType.IsAbstract)
                    complexType.IsAbstract = false;
            }
        }

        private void MarkTypesAbstract(CodeCompileUnit compileUnit)
        {
            foreach (var complexType in _serviceModel.ComplexTypes)
            {
                if (!complexType.IsAbstract)
                    continue;

                var typeReference = _xsdDataContractImporter.GetCodeTypeReference(complexType.QualifiedName);

                CodeTypeName typeName = typeReference.BaseType;

                var typeDeclaration = typeName.Find(compileUnit);
                if (typeDeclaration == null)
                    throw new Exception($"Declaration for type '{typeName}' not found in compile unit.");

                typeDeclaration.TypeAttributes |= TypeAttributes.Abstract;
            }
        }
    }
}
