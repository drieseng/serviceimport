using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;
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
    public class ServiceImportTask : Task
    {
        public ITaskItem[] NamespaceMappings
        {
            get; set;
        }

        /// <summary>
        /// Allow fine-grained control over the nillable characteristic of specific elements within
        /// complex types.
        /// </summary>
        public ITaskItem[] NillableOverrides
        {
            get; set;
        }

        /// <summary>
        /// Allow fine-grained control over the <see cref="DataMemberAttribute.IsRequired"/> characteristic of members.
        /// </summary>
        public ITaskItem[] IsRequiredMemberOverrides
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

        public ITaskItem DataContractGenerationOptions
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
            var xmlTypeMappings = CreateXmlTypeMappings();
            var namespaceMappings = CreateNamespaceMappings();
            var typeAccessModifierMappings = CreateTypeAccessModifierMappings();
            var typeRenameMappings = CreateTypeRenameMappings();
            var nillableOverrides = CreateNillableOverrides();
            var requiredMemberOverrides = CreateRequiredMemberOverrides();
            var codeWriter = new FileSystemCodeWriter(codeGeneratorOptions, OutputDirectory);
            var serviceImporter = new ServiceImporter(Wsdls.ToStringArray(),
                                                      xmlTypeMappings,
                                                      namespaceMappings,
                                                      nillableOverrides,
                                                      requiredMemberOverrides,
                                                      typeAccessModifierMappings,
                                                      typeRenameMappings);
            serviceImporter.DataContractGenerationOptions = new DataContractGenerationOptionsFactory().Create(DataContractGenerationOptions);
            serviceImporter.ServiceContractGenerationOptions = new ServiceContractGenerationOptionsFactory().Create(ServiceContractGenerationOptions); ;

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

        private Dictionary<XmlQualifiedName, Dictionary<string, NillableOverride>> CreateNillableOverrides()
        {
            var overrides = new Dictionary<XmlQualifiedName, Dictionary<string, NillableOverride>>();

            if (NillableOverrides != null)
            {
                var factory = new NillableOverrideFactory(new XmlQualifiedNameFactory());

                foreach (var item in NillableOverrides)
                {
                    var nillableOverride = factory.Create(item);

                    if (!overrides.TryGetValue(nillableOverride.ComplexTypeName, out var overridesForComplexType))
                    {
                        overridesForComplexType = new Dictionary<string, NillableOverride>();
                        overrides.Add(nillableOverride.ComplexTypeName, overridesForComplexType);
                    }

                    if (overridesForComplexType.ContainsKey(nillableOverride.ElementName))
                    {
                        throw new ArgumentException($"Duplicate nillable override for element '{nillableOverride.ElementName}' in complex type '{nillableOverride.ComplexTypeName}'.");
                    }

                    overridesForComplexType.Add(nillableOverride.ElementName, nillableOverride);
                }
            }

            return overrides;
        }

        private List<IsRequiredMemberOverride> CreateRequiredMemberOverrides()
        {
            var overrides = new List<IsRequiredMemberOverride>();

            if (IsRequiredMemberOverrides != null)
            {
                var factory = new IsRequiredMemberOverrideFactory();

                foreach (var item in IsRequiredMemberOverrides)
                {
                    var requiredMemberOverride = factory.Create(item);
                    overrides.Add(requiredMemberOverride);
                }
            }

            return overrides;
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

                    if (xmlTypeMappings.ContainsKey(xmlTypeMapping.XmlTypeCode))
                    {
                        Log.LogError($"XmlTypeMapping for '{xmlTypeMapping.XmlTypeCode}' is defined more than once.");
                        continue;
                    }

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
    }
}
