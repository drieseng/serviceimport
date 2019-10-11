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
    /// Updates <see cref="DataMemberAttribute.EmitDefaultValue"/> to <c>true</c> on properties for which
    /// <see cref="DataMemberAttribute.IsRequired"/> is <c>true</c> (because &quot;minOccurs&quot; on the
    /// corresponding element was set to <c>1</c>) and which are either a struct or an enum (which defines
    /// a field with value <c>0</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This extension is necessary to ensure an XML element is effectively emitted in case the value of a
    /// required property corresponds with the default value of a struct or enum property type (eg. <c>0</c> for an
    /// <see cref="int"/> or enum, or <c>null</c> for a property backed by a reference type).
    /// </para>
    /// <para>
    /// For an <see cref="Enum"/> that does not define a field with value <c>0</c>, we will not emit a default
    /// value as we want this to fail if no &quot;real&quot; value is assigned the property.
    /// </para>
    /// </remarks>
    public class EmitDefaultValueExtension : IWsdlImportExtension, IServiceContractGenerationExtension, IContractBehavior, IXsdImportExtension
    {
        private readonly ServiceModel _serviceModel;
        private readonly IDictionary<XmlTypeCode, XmlTypeMapping> _xmlTypeMappings;
        private XsdDataContractImporter _xsdDataContractImporter;

        public EmitDefaultValueExtension(ServiceModel serviceModel, IDictionary<XmlTypeCode, XmlTypeMapping> xmlTypeMappings)
        {
            _serviceModel = serviceModel;
            _xmlTypeMappings = xmlTypeMappings;
        }

        #region IWsdlImportExtension implementation

        void IWsdlImportExtension.BeforeImport(ServiceDescriptionCollection wsdlDocuments, XmlSchemaSet xmlSchemas, ICollection<XmlElement> policy)
        {
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
        }

        void IXsdImportExtension.ImportContract(XsdDataContractImporter importer)
        {
            _xsdDataContractImporter = importer;
        }

        #endregion IXsdImportExtension implementation

        #region IDataContractGenerationExtension implementation

        void IDataContractGenerationExtension.GenerateContract(CodeCompileUnit compileUnit)
        {
            FixEmitDefaultValue(compileUnit);
        }

        #endregion IDataContractGenerationExtension implementation

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

                    var emitDefaultValue = IsRequired(dataMemberAttribute) &&
                                           !ConsiderDefaultValueForTypeAsNoValue(compileUnit, _xmlTypeMappings, property.Type.BaseType);

                    var emitDefaultValueArgument = dataMemberAttribute.Arguments.FindArgumentByName(nameof(DataMemberAttribute.EmitDefaultValue));
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
            var isRequiredArgument = dataMemberAttribute.Arguments.FindArgumentByName(nameof(DataMemberAttribute.IsRequired));
            if (isRequiredArgument == null)
                return false;

            var value = isRequiredArgument.Value;
            if (value == null)
                throw new ArgumentException($"The value of '{nameof(DataMemberAttribute.IsRequired)}' cannot be null.", nameof(dataMemberAttribute));

            if (!(value is CodePrimitiveExpression primitiveExpression))
                throw new ArgumentException($"The value of '{nameof(DataMemberAttribute.IsRequired)}' should be a primitive expression.", nameof(dataMemberAttribute));

            var primitiveValue = primitiveExpression.Value;
            if (primitiveValue == null)
                throw new ArgumentException($"The primitive value of '{nameof(DataMemberAttribute.IsRequired)}' cannot be null.", nameof(dataMemberAttribute));

            if (primitiveValue is bool isRequired)
            {
                return isRequired;
            }

            throw new ArgumentException($"The primitive value of '{nameof(DataMemberAttribute.IsRequired)}' should be of type '{typeof(bool).FullName}'.", nameof(dataMemberAttribute));
        }

        private static bool ConsiderDefaultValueForTypeAsNoValue(CodeCompileUnit compileUnit, IDictionary<XmlTypeCode, XmlTypeMapping> xmlTypeMappings, string typeName)
        {
            if (typeName == typeof(short).FullName)
            {
                return false;
            }

            if (typeName == typeof(int).FullName)
            {
                return false;
            }

            if (typeName == typeof(long).FullName)
            {
                return false;
            }

            if (typeName == typeof(float).FullName)
            {
                return false;
            }

            if (typeName == typeof(ushort).FullName)
            {
                return false;
            }

            if (typeName == typeof(uint).FullName)
            {
                return false;
            }

            if (typeName == typeof(ulong).FullName)
            {
                return false;
            }

            if (typeName == typeof(decimal).FullName)
            {
                return false;
            }

            if (typeName == typeof(bool).FullName)
            {
                return false;
            }

            if (typeName == typeof(DateTime).FullName)
            {
                return false;
            }

            if (typeName == typeof(Guid).FullName)
            {
                return false;
            }

            if (typeName == typeof(TimeSpan).FullName)
            {
                return false;
            }

            if (typeName == typeof(string).FullName)
            {
                return false;
            }

            if (typeName == "System.Collections.Generic.Dictionary`2")
            {
                return true;
            }

            if (typeName == "System.Collections.Generic.List`1")
            {
                return true;
            }

            var typeDeclaration = compileUnit.FindTypeDeclaration(typeName);
            if (typeDeclaration == null)
            {
                var typeMapping = FindTypeMapping(xmlTypeMappings, typeName);
                if (typeMapping == null)
                {
                    throw new Exception($"Unable to determine whether '{typeName}' is a struct or enum.");
                }

                return !(typeMapping.IsStruct || typeMapping.IsEnum);
            }

            if (typeDeclaration.IsEnum)
            {
                foreach (CodeMemberField field in typeDeclaration.Members)
                {
                    if (field.InitExpression is CodePrimitiveExpression primitiveExpression)
                    {
                        if (primitiveExpression.Value is long longValue)
                        {
                            // If one of the values defined by the num has value zero, then we consider the
                            // default value as meaningful
                            if (longValue == 0L)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            throw new Exception($"Type '{primitiveExpression.Value.GetType()}' is not implemented.");
                        }
                    }
                }

                return false;
            }

            return typeDeclaration.IsStruct;
        }

        private static XmlTypeMapping FindTypeMapping(IDictionary<XmlTypeCode, XmlTypeMapping> xmlTypeMappings, string typeName)
        {
            foreach (var xmlTypeMapping in xmlTypeMappings.Values)
            {
                if (xmlTypeMapping.CodeTypeReference.BaseType == typeName)
                {
                    return xmlTypeMapping;
                }
            }

            return null;
        }
    }
}
