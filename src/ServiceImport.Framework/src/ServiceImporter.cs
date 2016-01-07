using BRail.Nis.ServiceImport.Framework.CodeDom;
using BRail.Nis.ServiceImport.Framework.Extension;
using BRail.Nis.ServiceImport.Framework.Factory;
using BRail.Nis.ServiceImport.Framework.Helper;
using BRail.Nis.ServiceImport.Framework.Writer;
using Microsoft.CSharp;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ServiceModel.Description;
using System.Xml.Schema;

namespace BRail.Nis.ServiceImport.Framework
{
    public class ServiceImporter
    {
        public ServiceImporter(string wsdl, IDictionary<XmlTypeCode, CodeTypeReference> xmlTypeMappings, IDictionary<string, string> namespaceMappings, IDictionary<string, TypeAccessModifier> typeAccessModifiers)
        {
            Wsdl = wsdl;
            XmlTypeMappings = xmlTypeMappings;
            NamespaceMappings = namespaceMappings;
            TypeAccessModifiers = typeAccessModifiers;
        }

        public string Wsdl
        {
            get; private set;
        }

        public IDictionary<XmlTypeCode, CodeTypeReference> XmlTypeMappings
        {
            get; private set;
        }

        public IDictionary<string, string> NamespaceMappings
        {
            get; private set;
        }


        public IDictionary<string, TypeAccessModifier> TypeAccessModifiers
        {
            get; private set;
        }

        public void Import(ICodeWriter codeWriter)
        {
            var codeCompileUnit = new CodeCompileUnit();
            var codeProvider = CreateCodeProvider();
            var metadataSet = new MetadataDiscovery().Discover(Wsdl);
            var xsdDataContractImporter = new XsdDataContractImporterFactory().Create(codeProvider, codeCompileUnit, NamespaceMappings);
            var wsdlImporter = new WsdlImporterFactory().Create(metadataSet, xsdDataContractImporter, XmlTypeMappings);
            var serviceContractGenerator = CreateServiceContractGenerator(codeCompileUnit, NamespaceMappings);

            wsdlImporter.ImportAllBindings();
            wsdlImporter.ImportAllEndpoints();

            foreach (var contract in wsdlImporter.ImportAllContracts())
                serviceContractGenerator.GenerateServiceContractType(contract);

            new TypeAccessModifierExtension().Apply(TypeAccessModifiers, codeCompileUnit);

            codeWriter.Write(codeProvider, codeCompileUnit);
        }

        private static CodeDomProvider CreateCodeProvider()
        {
            var options = new Dictionary<string, string> { { "CompilerVersion", "v4.5" } };
            return new CSharpCodeProvider(options);
        }

        private static ServiceContractGenerator CreateServiceContractGenerator(CodeCompileUnit codeCompileUnit, IDictionary<string, string> namespaceMappings)
        {
            var gen = new ServiceContractGenerator(codeCompileUnit)
                {
                    Options = ServiceContractGenerationOptions.ClientClass
                };
            foreach (var namespaceMapping in namespaceMappings)
                gen.NamespaceMappings.Add(namespaceMapping.Key, namespaceMapping.Value);
            return gen;
        }
    }
}
