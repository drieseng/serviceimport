using System.CodeDom;
using System.CodeDom.Compiler;

namespace BRail.Nis.ServiceImport.Framework.Writer
{
    public interface ICodeWriter
    {
        void Write(CodeDomProvider codeDomProvider, CodeCompileUnit codeCompileUnit);
    }
}
