using System.CodeDom.Compiler;

namespace ServiceImport.Console
{
    public class CodeGeneratorOptionsFactory
    {
        public CodeGeneratorOptions Create()
        {
            return new CodeGeneratorOptions
                {
                    BlankLinesBetweenMembers = true,
                    BracingStyle = "C",
                    ElseOnClosing = false
                };
        }
    }
}
