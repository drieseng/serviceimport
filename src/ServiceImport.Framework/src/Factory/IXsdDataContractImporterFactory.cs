using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ServiceImport.Framework.Factory
{
    public interface IXsdDataContractImporterFactory
    {
        XsdDataContractImporter Create(CodeDomProvider codeDomProvider, CodeCompileUnit codeCompileUnit, IDictionary<string, string> namespaceMappings);
    }
}
