using ServiceImport.Framework.CodeDom;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Schema;
using ServiceImport.Framework.Documentation;
using ServiceImport.Framework.Model;
using ImportOptions = ServiceImport.Framework.Documentation.ImportOptions;

namespace ServiceImport.Framework.Extension
{
    public class XmlCommentExtension : IXsdImportExtension, IWsdlImportExtension, IServiceContractGenerationExtension, IContractBehavior
    {
        private readonly ServiceModel _serviceModel;

        public XmlCommentExtension(ServiceModel serviceModel)
        {
            _serviceModel = serviceModel;
        }

        #region IWsdlImportExtension implementation

        void IWsdlImportExtension.BeforeImport(ServiceDescriptionCollection wsdlDocuments, XmlSchemaSet xmlSchemas, ICollection<XmlElement> policy)
        {
        }

        void IWsdlImportExtension.ImportContract(WsdlImporter importer, WsdlContractConversionContext context)
        {
            context.Contract.ContractBehaviors.Add(this);
        }

        void IWsdlImportExtension.ImportEndpoint(WsdlImporter importer, WsdlEndpointConversionContext context)
        {
        }

        #endregion IWsdlImportExtension implementation

        #region IServiceContractGenerationExtension implementation

        public void GenerateContract(ServiceContractGenerationContext context)
        {
            var compileUnit = context.ServiceContractGenerator.TargetCompileUnit;
            GenerateContract(compileUnit);
        }

        #endregion IServiceContractGenerationExtension implementation

        #region IXsdImportExtension implementation

        public void GenerateContract(CodeCompileUnit compileUnit)
        {
            //Console.WriteLine("IN GENERATE CONTRACT");

            foreach (var ns in compileUnit.Namespaces())
            {
                //Console.WriteLine("CODEDOM NS = " + ns.Name);

                foreach (var typeDeclaration in ns.Types())
                {
                    var qualifiedName = GetXmlQualifiedName(typeDeclaration);
                    if (qualifiedName == null)
                    {
                        continue;
                    }

                    //Console.WriteLine("\tCODEDOM TYPE = " + typeDeclaration.Name);


                    var schemaType = GetSchemaType(qualifiedName);
                    if (schemaType == null)
                        throw new Exception("TODO");

                    var xmlComment = GetDocumentation(schemaType.Annotation);
                    if (xmlComment != null)
                    {
                        typeDeclaration.Comments.Clear();
                        XmlCommentsImporter.AddXmlComment(typeDeclaration, xmlComment, new ImportOptions { Format = XmlCommentFormat.Portable });
                    }

                    if (typeDeclaration.IsEnum)
                    {
                        var docByMember = GetEnumerationDocumentationByMember(schemaType);

                        foreach (CodeTypeMember member in typeDeclaration.Members)
                        {
                            if (docByMember.TryGetValue(member.Name, out var memberDoc))
                            {
                                member.Comments.Clear();
                                XmlCommentsImporter.AddXmlComment(member, memberDoc, new ImportOptions {Format = XmlCommentFormat.Portable});
                            }
                        }
                    }

                }
            }
        }

        public void BeforeImport(XmlSchemaSet xmlSchemas)
        {
        }

        public void ImportContract(XsdDataContractImporter importer)
        {
        }

        #endregion IXsdImportExtension implementation

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

        private Dictionary<string, string> GetEnumerationDocumentationByMember(XmlSchemaType schemaType)
        {
            if (!(schemaType is XmlSchemaSimpleType simpleType))
            {
                throw new Exception();
            }

            var docByMember = new Dictionary<string, string>();

            if (simpleType.Content is XmlSchemaSimpleTypeRestriction restriction)
            {
                //Console.WriteLine("\t\t" + simpleType.Name);

                foreach (var facet in restriction.Facets)
                {
                    if (facet is XmlSchemaEnumerationFacet enumeration)
                    {
                        var xmlComment = GetDocumentation(enumeration.Annotation);
                        if (xmlComment != null)
                        {
                            //Console.WriteLine("\t\t\t" + simpleType.Name + " | " + enumeration.Value + " | " + xmlComment);
                            docByMember.Add(enumeration.Value, xmlComment);
                        }
                    }
                }
            }

            return docByMember;
        }

        private XmlSchemaType GetSchemaType(XmlQualifiedName qualifiedName)
        {
            foreach (XmlSchema schema in _serviceModel.XmlSchemas.Schemas())
            {
                if (schema.TargetNamespace != qualifiedName.Namespace)
                {
                    continue;
                }

                foreach (var schemaObject in schema.Items)
                {
                    if (!(schemaObject is XmlSchemaType schemaType) || schemaType.Name != qualifiedName.Name)
                    {
                        continue;
                    }

                    return schemaType;
                }
            }

            return null;
        }

        private static XmlQualifiedName GetXmlQualifiedName(CodeTypeDeclaration typeDeclaration)
        {
            var dataContractAttribute = typeDeclaration.GetDataContractAttribute();
            if (dataContractAttribute == null)
            {
                return null;
            }

            string name;

            var dataContractNameArgument = dataContractAttribute.FindArgumentByName("Name");
            if (dataContractNameArgument != null)
            {
                name = CodeDomExtensions.GetStringValue(dataContractNameArgument);
            }
            else
            {
                name = typeDeclaration.Name;
            }

            var dataContractNamespaceArgument = dataContractAttribute.FindArgumentByName("Namespace");
            if (dataContractNamespaceArgument == null)
            {
                throw new Exception();
            }

            return new XmlQualifiedName(name, CodeDomExtensions.GetStringValue(dataContractNamespaceArgument));
        }

        private static string GetDocumentation(XmlSchemaAnnotation annotation)
        {
            if (annotation == null)
                return null;

            foreach (var annotationItem in annotation.Items)
            {
                if (annotationItem is XmlSchemaDocumentation doc && doc.Markup.Length > 0)
                {
                    var documentation = new StringBuilder();
                    foreach (var node in doc.Markup)
                    {
                        documentation.Append(node.Value);
                    }
                    return documentation.ToString();
                }
            }
            return null;
        }
    }
}
