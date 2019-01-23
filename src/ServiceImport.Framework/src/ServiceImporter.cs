using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ServiceModel.Description;
using System.Xml;
using System.Xml.Schema;
using Microsoft.CSharp;
using ServiceImport.Framework.CodeDom;
using ServiceImport.Framework.Extension;
using ServiceImport.Framework.Factory;
using ServiceImport.Framework.Helper;
using ServiceImport.Framework.Model;
using ServiceImport.Framework.Writer;

namespace ServiceImport.Framework
{
    public class ServiceImporter
    {
        public ServiceImporter(string[] wsdls, IDictionary<XmlTypeCode, XmlTypeMapping> xmlTypeMappings,
            IDictionary<string, string> namespaceMappings, Dictionary<XmlQualifiedName, Dictionary<string, NillableOverride>> nillableOverrides,
            IDictionary<string, TypeAccessModifier> typeAccessModifiers, IDictionary<string, string> typeRenameMappings)
        {
            Wsdls = wsdls;
            XmlTypeMappings = xmlTypeMappings;
            NamespaceMappings = namespaceMappings;
            NillableOverrides = nillableOverrides;
            TypeAccessModifiers = typeAccessModifiers;
            TypeRenameMappings = typeRenameMappings;
        }

        public string[] Wsdls { get; }

        public IDictionary<XmlTypeCode, XmlTypeMapping> XmlTypeMappings { get; }

        public IDictionary<string, string> NamespaceMappings { get; }
        public Dictionary<XmlQualifiedName, Dictionary<string, NillableOverride>> NillableOverrides { get; }

        public DataContractGenerationOptions DataContractGenerationOptions { get; set; }

        public ServiceContractGenerationOptions ServiceContractGenerationOptions { get; set; }

        public IDictionary<string, TypeAccessModifier> TypeAccessModifiers { get; }

        public IDictionary<string, string> TypeRenameMappings { get; }

        public void Import(ICodeWriter codeWriter)
        {
            var dataContractGenerationExtensions = new List<IXsdImportExtension>
                {
                    new TypeRenameExtension(TypeRenameMappings),
                    new RemoveExtraDataContractNameExtension(),
                    new TypeAccessModifierExtension(TypeAccessModifiers),
                };

            var codeCompileUnit = new CodeCompileUnit();
            var codeProvider = CreateCodeProvider();

            var discovery = new MetadataDiscovery();
            var metadataSections = new List<MetadataSection>();

            foreach (var wsdl in Wsdls)
            {
                var metadataSet = discovery.Discover(wsdl);
                metadataSections.AddRange(metadataSet.MetadataSections);
            }

            var xsdDataContractImporter = new XsdDataContractImporterFactory().Create(codeProvider, codeCompileUnit, NamespaceMappings);

            var wsdlImporter = new WsdlImporterFactory().Create(new MetadataSet(metadataSections),
                                                                xsdDataContractImporter,
                                                                NillableOverrides,
                                                                XmlTypeMappings,
                                                                DataContractGenerationOptions);
            var serviceContractGenerator = CreateServiceContractGenerator(codeCompileUnit,
                                                                          NamespaceMappings,
                                                                          ServiceContractGenerationOptions);

            wsdlImporter.ImportAllBindings();
            wsdlImporter.ImportAllEndpoints();

            foreach (var contract in wsdlImporter.ImportAllContracts())
                serviceContractGenerator.GenerateServiceContractType(contract);

            foreach (var dataContractGenerationExtension in dataContractGenerationExtensions)
            {
                dataContractGenerationExtension.GenerateContract(codeCompileUnit);
            }

            codeWriter.Write(codeProvider, codeCompileUnit);
        }

        private static CodeDomProvider CreateCodeProvider()
        {
            var options = new Dictionary<string, string> { { "CompilerVersion", "v4.5" } };
            return new CSharpCodeProvider(options);
        }

        private static ServiceContractGenerator CreateServiceContractGenerator(CodeCompileUnit codeCompileUnit,
                                                                               IDictionary<string, string> namespaceMappings,
                                                                               ServiceContractGenerationOptions serviceContractGenerationOptions)
        {
            var gen = new ServiceContractGenerator(codeCompileUnit)
                {
                    Options = serviceContractGenerationOptions
                };
            foreach (var namespaceMapping in namespaceMappings)
                gen.NamespaceMappings.Add(namespaceMapping.Key, namespaceMapping.Value);
            return gen;
        }
    }
}
