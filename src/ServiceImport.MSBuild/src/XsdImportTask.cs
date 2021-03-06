﻿using System.Collections.Generic;
using System.Xml.Schema;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using ServiceImport.Framework;
using ServiceImport.Framework.CodeDom;
using ServiceImport.Framework.Model;
using ServiceImport.Framework.Writer;
using ServiceImport.MSBuild.Extension;
using ServiceImport.MSBuild.Factory;

namespace ServiceImport.MSBuild
{
    public class XsdImportTask : Task
    {
        public ITaskItem[] NamespaceMappings
        {
            get; set;
        }

        public ITaskItem[] XmlTypeMappings
        {
            get; set;
        }

        public ITaskItem[] TypeAccessModifierMappings
        {
            get;
            set;
        }

        public ITaskItem[] TypeRenameMappings
        {
            get;
            set;
        }

        public ITaskItem CodeGeneratorOptions
        {
            get; set;
        }

        [Required]
        public ITaskItem[] Xsds
        {
            get; set;
        }

        [Required]
        public string OutputDirectory
        {
            get; set;
        }

        public override bool Execute()
        {
            var codeGeneratorOptions = new CodeGeneratorOptionsFactory().Create(CodeGeneratorOptions);
            var xmlTypeMappings = CreateXmlTypeMappings();
            var namespaceMappings = CreateNamespaceMappings();
            var typeAccessModifierMappings = CreateTypeAccessModifierMappings();
            var typeRenameMappings = CreateTypeRenameMappings();
            var codeWriter = new FileSystemCodeWriter(codeGeneratorOptions, OutputDirectory);
            var xsdImporter = new XsdImporter(Xsds.ToStringArray(),
                                              xmlTypeMappings,
                                              namespaceMappings,
                                              typeAccessModifierMappings,
                                              typeRenameMappings);

            xsdImporter.Import(codeWriter);

            return true;
        }

        private IDictionary<string, string> CreateNamespaceMappings()
        {
            var namespaceMappings = new Dictionary<string, string>();

            if (NamespaceMappings != null)
            {
                var namespaceMappingFactory = new NamespaceMappingFactory();

                foreach (var item in NamespaceMappings)
                {
                    var namespaceMapping = namespaceMappingFactory.Create(item);
                    namespaceMappings.Add(namespaceMapping.TargetNamespace, namespaceMapping.ClrNamespace);
                }
            }

            return namespaceMappings;
        }

        private IDictionary<XmlTypeCode, XmlTypeMapping> CreateXmlTypeMappings()
        {
            var xmlTypeMappings = new Dictionary<XmlTypeCode, XmlTypeMapping>();

            if (XmlTypeMappings != null)
            {
                var xmlTypeMappingFactory = new XmlTypeMappingFactory();

                foreach (var item in XmlTypeMappings)
                {
                    var xmlTypeMapping = xmlTypeMappingFactory.Create(item);
                    xmlTypeMappings.Add(xmlTypeMapping.XmlTypeCode, xmlTypeMapping);
                }
            }

            return xmlTypeMappings;
        }

        private IDictionary<string, TypeAccessModifier> CreateTypeAccessModifierMappings()
        {
            var typeAccessModifierMappings = new Dictionary<string, TypeAccessModifier>();

            if (TypeAccessModifierMappings != null)
            {
                var typeAccessModifierMappingsFactory = new TypeAccessModifierMappingFactory();

                foreach (var item in TypeAccessModifierMappings)
                {
                    var typeAccessModifierMapping = typeAccessModifierMappingsFactory.Create(item);
                    typeAccessModifierMappings.Add(typeAccessModifierMapping.TypeName, typeAccessModifierMapping.AccessModifier);
                }
            }

            return typeAccessModifierMappings;
        }

        private IDictionary<string, string> CreateTypeRenameMappings()
        {
            var typeRenameMappings = new Dictionary<string, string>();

            if (TypeRenameMappings != null)
            {
                var typeRenameMappingFactory = new TypeRenameMappingFactory();

                foreach (var item in TypeRenameMappings)
                {
                    var typeRenameMapping = typeRenameMappingFactory.Create(item);
                    typeRenameMappings.Add(typeRenameMapping.OriginalTypeName, typeRenameMapping.NewTypeName);
                }
            }

            return typeRenameMappings;
        }
    }
}
