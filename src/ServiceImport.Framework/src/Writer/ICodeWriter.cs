using System.CodeDom;
using System.CodeDom.Compiler;

namespace ServiceImport.Framework.Writer
{
    public interface ICodeWriter
    {
        void Write(CodeDomProvider codeDomProvider, CodeCompileUnit codeCompileUnit);
    }
}
