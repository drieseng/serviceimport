﻿using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ServiceModel.Description;
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
    public class XsdImporter
    {
        public XsdImporter(string[] xsds, IDictionary<XmlTypeCode, XmlTypeMapping> xmlTypeMappings, IDictionary<string, string> namespaceMappings, IDictionary<string, TypeAccessModifier> typeAccessModifiers, IDictionary<string, string> typeRenameMappings)
        {
            Xsds = xsds;
            XmlTypeMappings = xmlTypeMappings;
            NamespaceMappings = namespaceMappings;
            TypeAccessModifiers = typeAccessModifiers;
            TypeRenameMappings = typeRenameMappings;
        }

        public string[] Xsds { get; }

        public IDictionary<XmlTypeCode, XmlTypeMapping> XmlTypeMappings
        {
            get; private set;
        }

        public IDictionary<string, string> NamespaceMappings
        {
            get; }


        public IDictionary<string, TypeAccessModifier> TypeAccessModifiers
        {
            get; }

        public IDictionary<string, string> TypeRenameMappings
        {
            get; }

        public void Import(ICodeWriter codeWriter)
        {
            var codeCompileUnit = new CodeCompileUnit();
            var codeProvider = CreateCodeProvider();
            var serviceModel = CreateServiceModel();

            var xsdImportExtensions = new List<IXsdImportExtension>
                {
                    new RemoveIExtensibleDataObjectImplementationExtension(),
                    new ServiceModelBuilderExtension(serviceModel),
                    new ComplexTypeElementTypeMappingExtension(serviceModel, XmlTypeMappings),
                    new TypeAccessModifierExtension(TypeAccessModifiers),
                    new TypeRenameExtension(TypeRenameMappings),
                    new ReplaceArrayOfTWithListTExtension(),
                    new RemoveExtraDataContractNameExtension(),
                    new ComplexTypeOptionalElementsNillableExtension(serviceModel),
                    new EmitDefaultValueExtension(serviceModel, XmlTypeMappings),
                    new AbstractTypeExtension(serviceModel),
                    new PascalCaseFieldNamesExtension()
                };

            foreach (var xsdImportExtension in xsdImportExtensions)
            {
                xsdImportExtension.BeforeImport(serviceModel.XmlSchemas);
            }

            var xsdDataContractImporter = new XsdDataContractImporterFactory().Create(codeProvider, codeCompileUnit, NamespaceMappings);

            foreach (var xsdImportExtension in xsdImportExtensions)
            {
                xsdImportExtension.ImportContract(xsdDataContractImporter);
            }

            xsdDataContractImporter.Import(serviceModel.XmlSchemas);

            foreach (var xsdImportExtension in xsdImportExtensions)
            {
                xsdImportExtension.GenerateContract(codeCompileUnit);
            }

            codeWriter.Write(codeProvider, codeCompileUnit);
        }

        private static XmlSchemaSet CreateXmlSchemaSet(List<MetadataSection> metadataSections)
        {
            var xmlSchemaSet = new XmlSchemaSet();

            foreach (var metadataSection in metadataSections)
            { 
                var xmlSchema = metadataSection.Metadata as XmlSchema;
                if (xmlSchema == null)
                    continue;

                if (xmlSchemaSet.Contains(xmlSchema.TargetNamespace))
                    continue;

                xmlSchemaSet.Add(xmlSchema);
            }

            xmlSchemaSet.Compile();

            return xmlSchemaSet;
        }

        private ServiceModel CreateServiceModel()
        {
            var discovery = new MetadataDiscovery();
            var metadataSections = new List<MetadataSection>();

            foreach (var xsd in Xsds)
            {
                var metadataSet = discovery.Discover(xsd);
                metadataSections.AddRange(metadataSet.MetadataSections);

            }
            var schemaSet = CreateXmlSchemaSet(metadataSections);
            return new ServiceModel { XmlSchemas = schemaSet };
        }

        private static CodeDomProvider CreateCodeProvider()
        {
            var options = new Dictionary<string, string> { { "CompilerVersion", "v4.5" } };
            return new CSharpCodeProvider(options);
        }
    }
}
