using BRail.Nis.ServiceImport.Framework.CodeDom;
using BRail.Nis.ServiceImport.Framework.Helper;
using BRail.Nis.ServiceImport.Framework.Model;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
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
    /// Marks elements of complex type as nillable before the import.
    /// </summary>
    /// <remarks>
    /// This is done to ensure the types of corresponding properties in the generated code are nullable in case of value types.
    /// </remarks>
    public class ComplexTypeOptionalElementsNillableExtension : IWsdlImportExtension, IServiceContractGenerationExtension, IContractBehavior
    {
        private readonly ServiceModel _serviceModel;
        private XsdDataContractImporter _xsdDataContractImporter;

        public ComplexTypeOptionalElementsNillableExtension(ServiceModel serviceModel)
        {
            _serviceModel = serviceModel;
        }

        #region IWsdlImportExtension implementation

        public void BeforeImport(ServiceDescriptionCollection wsdlDocuments, XmlSchemaSet xmlSchemas, ICollection<XmlElement> policy)
        {
            foreach (DictionaryEntry globalTypeEntry in _serviceModel.XmlSchemas.GlobalTypes)
            {
                var globalType = globalTypeEntry.Value;

                var complexType = globalType as XmlSchemaComplexType;
                if (complexType == null || complexType.Name == null)
                    continue;

                foreach (var element in complexType.GetSequenceElements())
                    if (element.MinOccurs == 0)
                        element.IsNillable = true;
            }
        }

        void IWsdlImportExtension.ImportContract(WsdlImporter importer, WsdlContractConversionContext context)
        {
            _xsdDataContractImporter = importer.Get<XsdDataContractImporter>();
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

            foreach (var complexType in _serviceModel.ComplexTypes)
            {
                var typeReference = _xsdDataContractImporter.GetCodeTypeReference(complexType.QualifiedName);

                var typeDeclaration = compileUnit.FindTypeDeclaration(typeReference.BaseType);

                // skip complex type for which no type was generated (eg. a List<>)
                if (typeDeclaration == null)
                    continue;

                foreach (var property in typeDeclaration.Properties())
                {
                    Element element;
                    if (!complexType.Elements.TryGetValue(property.Name, out element))
                        throw new Exception();

                    if (element.MaxOccurs != 1 || element.MinOccurs != 0)
                        continue;

                    var dataMemberAttribute = property.CustomAttributes.SingleOrDefault<CodeAttributeDeclaration>(p => p.Name == typeof(DataMemberAttribute).FullName);
                    if (dataMemberAttribute == null)
                        throw new Exception();

                    var emitDefaultValueArgument = dataMemberAttribute.Arguments.SingleOrDefault<CodeAttributeArgument>(p => p.Name == "EmitDefaultValue");
                    if (emitDefaultValueArgument == null)
                    {
                        emitDefaultValueArgument = new CodeAttributeArgument("EmitDefaultValue", new CodePrimitiveExpression(false));
                        dataMemberAttribute.Arguments.Add(emitDefaultValueArgument);
                    }
                    else
                    {
                        emitDefaultValueArgument.Value = new CodePrimitiveExpression(false);
                    }
                }
            }
        }

        #endregion IServiceContractGenerationExtension implementation
    }
}
