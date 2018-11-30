using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using ServiceImport.Framework.CodeDom;

namespace ServiceImport.Framework.Writer
{
    public class FileSystemCodeWriter : ICodeWriter
    {
        public FileSystemCodeWriter(CodeGeneratorOptions codeGeneratorOptions, string outputDirectory)
        {
            CodeGeneratorOptions = codeGeneratorOptions;
            OutputDirectory = outputDirectory;
        }

        public CodeGeneratorOptions CodeGeneratorOptions
        {
            get; private set;
        }

        public string OutputDirectory
        {
            get; private set;
        }

        public void Write(CodeDomProvider codeDomProvider, CodeCompileUnit codeCompileUnit)
        {
            foreach (CodeNamespace ns in codeCompileUnit.Namespaces)
            {
                var namespaceParts = ns.Name.Split('.');

                var namespaceOutputDirectory = Path.Combine(OutputDirectory, string.Join("" + Path.DirectorySeparatorChar, namespaceParts));
                if (!Directory.Exists(namespaceOutputDirectory))
                    Directory.CreateDirectory(namespaceOutputDirectory);

                foreach (CodeTypeDeclaration x in ns.Types)
                {
                    var compileUnit = new CodeCompileUnit();
                    var nsType = ns.Clone();
                    nsType.Types.Add(x);
                    compileUnit.Namespaces.Add(nsType);

                    var typeOutputFile = Path.Combine(namespaceOutputDirectory, x.Name + "." + codeDomProvider.FileExtension);

                    using (var sw = new StreamWriter(File.Open(typeOutputFile, FileMode.CreateNew, FileAccess.ReadWrite)))
                    {
                        codeDomProvider.GenerateCodeFromCompileUnit(compileUnit, sw, CodeGeneratorOptions);
                    }
                }
            }
        }
    }
}
