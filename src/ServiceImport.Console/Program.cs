using BRail.Nis.ServiceImport.Framework;
using BRail.Nis.ServiceImport.Framework.Writer;

namespace ServiceImport.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var options = new OptionsFactory().Create(args);
            var codeGeneratorOptions = new CodeGeneratorOptionsFactory().Create();
            var codeWriter = new FileSystemCodeWriter(codeGeneratorOptions, options.OutputDirectory);
            var serviceImporter = new ServiceImporter(options.Wsdl, options.XmlTypeMappings, options.NamespaceMappings);

            serviceImporter.Import(codeWriter);
        }
    }
}
