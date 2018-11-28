﻿using System.CodeDom;
using System.Collections.Generic;
using System.Xml.Schema;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using ServiceImport.Framework;
using ServiceImport.Framework.CodeDom;
using ServiceImport.Framework.Writer;
using ServiceImport.MSBuild.Factory;

namespace ServiceImport.MSBuild
{
    public class ServiceImportTask : Task
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

        public ITaskItem ServiceContractGenerationOptions
        {
            get; set;
        }

        [Required]
        public ITaskItem[] Wsdls
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
            var serviceContractGenerationOptions = new ServiceContractGenerationOptionsFactory().Create(ServiceContractGenerationOptions);
            var xmlTypeMappings = CreateXmlTypeMappings();
            var namespaceMappings = CreateNamespaceMappings();
            var typeAccessModifierMappings = CreateTypeAccessModifierMappings();
            var typeRenameMappings = CreateTypeRenameMappings();
            var codeWriter = new FileSystemCodeWriter(codeGeneratorOptions, OutputDirectory);
            var serviceImporter = new ServiceImporter(GetItemSpecs(Wsdls),
                                                      xmlTypeMappings,
                                                      namespaceMappings,
                                                      typeAccessModifierMappings,
                                                      typeRenameMappings);
            serviceImporter.ServiceContractGenerationOptions = serviceContractGenerationOptions;

            serviceImporter.Import(codeWriter);

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

                    if (namespaceMappings.ContainsKey(namespaceMapping.TargetNamespace))
                    {
                        Log.LogError($"NamespaceMapping for '{namespaceMapping.TargetNamespace}' is defined more than once.");
                        continue;
                    }

                    namespaceMappings.Add(namespaceMapping.TargetNamespace, namespaceMapping.ClrNamespace);
                }
            }

            return namespaceMappings;
        }

        private IDictionary<XmlTypeCode, CodeTypeReference> CreateXmlTypeMappings()
        {
            var xmlTypeMappings = new Dictionary<XmlTypeCode, CodeTypeReference>();

            if (XmlTypeMappings != null)
            {
                var xmlTypeMappingFactory = new XmlTypeMappingFactory();

                foreach (var item in XmlTypeMappings)
                {
                    var xmlTypeMapping = xmlTypeMappingFactory.Create(item);

                    if (xmlTypeMappings.ContainsKey(xmlTypeMapping.XmlTypeCode))
                    {
                        Log.LogError($"XmlTypeMapping for '{xmlTypeMapping.XmlTypeCode}' is defined more than once.");
                        continue;
                    }

                    xmlTypeMappings.Add(xmlTypeMapping.XmlTypeCode, xmlTypeMapping.CodeTypeReference);
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

                    if (typeAccessModifierMappings.ContainsKey(typeAccessModifierMapping.TypeName))
                    {
                        Log.LogError($"TypeAccessModifierMapping for '{typeAccessModifierMapping.TypeName}' is defined more than once.");
                        continue;
                    }

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

                    if (typeRenameMappings.ContainsKey(typeRenameMapping.OriginalTypeName))
                    {
                        Log.LogError($"TypeRenameMapping for '{typeRenameMapping.OriginalTypeName}' is defined more than once.");
                        continue;
                    }

                    typeRenameMappings.Add(typeRenameMapping.OriginalTypeName, typeRenameMapping.NewTypeName);
                }
            }

            return typeRenameMappings;
        }

        private static string[] GetItemSpecs(ITaskItem[] taskItems)
        {
            var itemSpecs = new string[taskItems.Length];
            for (var i = 0; i < taskItems.Length; i++)
            {
                var taskItem = taskItems[i];
                itemSpecs[i] = taskItem.ItemSpec;
            }

            return itemSpecs;
        }
    }
}
