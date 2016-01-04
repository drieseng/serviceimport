using BRail.Nis.ServiceImport.Framework.CodeDom;
using BRail.Nis.ServiceImport.Framework.Helper;
using BRail.Nis.ServiceImport.Framework.Model;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Schema;

namespace BRail.Nis.ServiceImport.Framework.Extension
{
    public class ComplexTypeElementTypeMappingExtension : IServiceContractGenerationExtension, IWsdlImportExtension, IContractBehavior
    {
        private XsdDataContractImporter _xsdDataContractImporter;
        private readonly IDictionary<XmlTypeCode, CodeTypeReference> _xmlTypeMapping;
        private readonly ServiceModel _serviceModel;

        public ComplexTypeElementTypeMappingExtension(ServiceModel serviceModel, IDictionary<XmlTypeCode, CodeTypeReference> xmlTypeMapping)
        {
            _serviceModel = serviceModel;
            _xmlTypeMapping = xmlTypeMapping;
        }

        #region IWsdlImportExtension implementation

        public void BeforeImport(ServiceDescriptionCollection wsdlDocuments, XmlSchemaSet xmlSchemas, ICollection<XmlElement> policy)
        {
        }

        public void ImportContract(WsdlImporter importer, WsdlContractConversionContext context)
        {
            _xsdDataContractImporter = importer.Get<XsdDataContractImporter>();

            context.Contract.ContractBehaviors.Add(this);
        }

        public void ImportEndpoint(WsdlImporter importer, WsdlEndpointConversionContext context)
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

                    if (element.MaxOccurs != 1)
                        continue;

                    CodeTypeReference elementTypeReference;
                    if (!_xmlTypeMapping.TryGetValue(element.SchemaType.TypeCode, out elementTypeReference))
                        continue;

                    if (element.MinOccurs == 0)
                    {
                        property.Type = elementTypeReference.ToNullable();
                    }
                    else
                    {
                        property.Type = elementTypeReference;
                    }

                    var returnStatement = property.GetStatements[0] as CodeMethodReturnStatement;
                    if (returnStatement != null && returnStatement.Expression != null)
                    {
                        var fieldReference = returnStatement.Expression as CodeFieldReferenceExpression;
                        if (fieldReference != null)
                        {
                            var field = typeDeclaration.Fields().SingleOrDefault(p => p.Name == fieldReference.FieldName);
                            if (field == null)
                                throw new Exception();

                            field.Type = property.Type;
                        }
                    }
                }

            }
        }

        #endregion IServiceContractGenerationExtension implementation
    }
}
