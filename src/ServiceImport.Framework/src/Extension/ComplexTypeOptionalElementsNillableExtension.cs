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
using ServiceImport.Framework.CodeDom;
using ServiceImport.Framework.Helper;
using ServiceImport.Framework.Model;

namespace ServiceImport.Framework.Extension
{
    /// <summary>
    /// Marks elements of complex type as nillable before the import.
    /// </summary>
    /// <remarks>
    /// This is done to ensure the types of corresponding properties in the generated code are nullable in case of value types.
    /// </remarks>
    public class ComplexTypeOptionalElementsNillableExtension : IWsdlImportExtension, IServiceContractGenerationExtension, IContractBehavior, IXsdImportExtension
    {
        private const string SerializationArraysNamespace = "http://schemas.microsoft.com/2003/10/Serialization/Arrays";

        private readonly ServiceModel _serviceModel;
        private XsdDataContractImporter _xsdDataContractImporter;

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

        void IServiceContractGenerationExtension.GenerateContract(ServiceContractGenerationContext context)
        {
            // use the compile unit of the ServiceContractGenerator to have types defined
            // in XSDs and WSDLs
            var compileUnit = context.ServiceContractGenerator.TargetCompileUnit;
            FixEmitDefaultValue(compileUnit);
        }

        #endregion IServiceContractGenerationExtension implementation

        #region IXsdImportExtension implementation

        void IXsdImportExtension.BeforeImport(XmlSchemaSet xmlSchemas)
        {
            MarkOptionalElementsNillable();
        }

        void IXsdImportExtension.ImportContract(XsdDataContractImporter importer)
        {
            _xsdDataContractImporter = importer;
        }

        void IDataContractGenerationExtension.GenerateContract(CodeCompileUnit compileUnit)
        {
            FixEmitDefaultValue(compileUnit);
        }

        #endregion IXsdImportExtension implementation

        private void MarkOptionalElementsNillable()
        {
            foreach (var complexType in _serviceModel.ComplexTypes)
            {
                // do not touch the IsNillable for the elements in specialized array serialization complex types,
                // if not the XsdDataContractImporter will generate wrapper classes for these arrays
                if (complexType.QualifiedName.Namespace == SerializationArraysNamespace)
                    continue;

                foreach (var element in complexType.Elements)
                    if (element.MinOccurs == 0)
                        element.IsNillable = true;
            }
        }

        /// <summary>
        /// For those properties for which <see cref="DataMemberAttribute.IsRequired"/> is <c>true</c>, set <see cref="DataMemberAttribute.EmitDefaultValue"/>
        /// to <c>true</c>; otherwise, set <see cref="DataMemberAttribute.EmitDefaultValue"/> to <c>false</c>.
        /// </summary>
        /// <param name="compileUnit">The compile unit.</param>
        private void FixEmitDefaultValue(CodeCompileUnit compileUnit)
        {
            foreach (var complexType in _serviceModel.ComplexTypes)
            {
                var typeReference = _xsdDataContractImporter.GetCodeTypeReference(complexType.QualifiedName);
                var typeDeclaration = compileUnit.FindTypeDeclaration(typeReference.BaseType);

                // skip complex type for which no type was generated (eg. a List<>)
                if (typeDeclaration == null)
                    continue;

                foreach (var property in typeDeclaration.Properties())
                {
                    var dataMemberAttribute = property.CustomAttributes.SingleOrDefault<CodeAttributeDeclaration>(p => p.Name == typeof(DataMemberAttribute).FullName);
                    if (dataMemberAttribute == null)
                        continue;

                    var emitDefaultValue = IsRequired(dataMemberAttribute);

                    var emitDefaultValueArgument = dataMemberAttribute.Arguments.SingleOrDefault<CodeAttributeArgument>(p => p.Name == nameof(DataMemberAttribute.EmitDefaultValue));
                    if (emitDefaultValueArgument == null)
                    {
                        emitDefaultValueArgument = new CodeAttributeArgument("EmitDefaultValue", new CodePrimitiveExpression(emitDefaultValue));
                        dataMemberAttribute.Arguments.Add(emitDefaultValueArgument);
                    }
                    else
                    {
                        emitDefaultValueArgument.Value = new CodePrimitiveExpression(emitDefaultValue);
                    }
                }
            }
        }

        private static bool IsRequired(CodeAttributeDeclaration dataMemberAttribute)
        {
            var isRequiredArgument = dataMemberAttribute.Arguments.SingleOrDefault<CodeAttributeArgument>(p => p.Name == nameof(DataMemberAttribute.IsRequired));
            if (isRequiredArgument == null)
                return false;

            var value = isRequiredArgument.Value;
            if (value == null)
                throw new ArgumentException($"The value of '{nameof(DataMemberAttribute.IsRequired)}' cannot be null.", nameof(dataMemberAttribute));

            var primitiveExpression = value as CodePrimitiveExpression;
            if (primitiveExpression == null)
                throw new ArgumentException($"The value of '{nameof(DataMemberAttribute.IsRequired)}' should be a primitive expression.", nameof(dataMemberAttribute));

            var primitiveValue = primitiveExpression.Value;
            if (primitiveValue == null)
                throw new ArgumentException($"The primitive value of '{nameof(DataMemberAttribute.IsRequired)}' cannot be null.", nameof(dataMemberAttribute));

            if (!(primitiveValue is bool))
            {
                throw new ArgumentException($"The primitive value of '{nameof(DataMemberAttribute.IsRequired)}' should be of type '{typeof(bool).FullName}'.", nameof(dataMemberAttribute));
            }

            return (bool) primitiveValue;
        }
    }
}
