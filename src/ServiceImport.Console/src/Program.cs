using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Xml.Schema;
using ServiceImport.Framework;
using ServiceImport.Framework.CodeDom;
using ServiceImport.Framework.Model;
using ServiceImport.Framework.Writer;

namespace ServiceImport.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var codeGeneratorOptions = CreateCodeGeneratorOptions();
            var xmlTypeMappings = CreateXmlTypeMappings();
            var namespaceMappings = CreateNamespaceMappings();
            var typeAccessModifierMappings = CreateTypeAccessModifierMappings();
            var typeRenameMappings = CreateTypeRenameMappings();
            var codeWriter = new FileSystemCodeWriter(codeGeneratorOptions, "c:\\temp");
            var xsdImporter = new XsdImporter(new[] {@"C:\TFS2010\A204-NIS\Contracts\A1403\Current\src\Schemas\content\Contracts\A1403\ProductionPoints_v1.xsd"}, xmlTypeMappings, namespaceMappings, typeAccessModifierMappings, typeRenameMappings);

            xsdImporter.Import(codeWriter);

            //var options = new OptionsFactory().Create(args);
            //var codeGeneratorOptions = new CodeGeneratorOptionsFactory().Create();
            //var codeWriter = new FileSystemCodeWriter(codeGeneratorOptions, options.OutputDirectory);
            //var serviceImporter = new ServiceImporter(options.Wsdl,
            //                                          options.XmlTypeMappings,
            //                                          options.NamespaceMappings,
            //                                          options.TypeAccessModifierMappings,
            //                                          options.TypeRenameMappings);

            //serviceImporter.Import(codeWriter);
        }

        private static CodeGeneratorOptions CreateCodeGeneratorOptions()
        {
            var codeGeneratorOptions = new CodeGeneratorOptions
            {
                BracingStyle = "C",
                BlankLinesBetweenMembers = true,
                ElseOnClosing = false
            };
            return codeGeneratorOptions;
        }

        private static IDictionary<string, string> CreateNamespaceMappings()
        {
            return new Dictionary<string, string>
            {
                { "http://www.infrabel.be/A1403/ProductionPoints/V1", "BRail.A1403.ContractEntities.ProductionPoints.V1" }
            };
        }

        private static IDictionary<XmlTypeCode, XmlTypeMapping> CreateXmlTypeMappings()
        {
            return new Dictionary<XmlTypeCode, XmlTypeMapping>
            {
                {
                    XmlTypeCode.Date, new XmlTypeMapping
                    {
                        CodeTypeReference = new CodeTypeReference("BRail.Nis.GeneralLib.ContractEntities.Date")
                    }
                }
            };
        }

        private static IDictionary<string, TypeAccessModifier> CreateTypeAccessModifierMappings()
        {
            return new Dictionary<string, TypeAccessModifier>();
        }

        private static IDictionary<string, string> CreateTypeRenameMappings()
        {
            var typeRenameMappings = new Dictionary<string, string>();
            return typeRenameMappings;
        }
    }
}
