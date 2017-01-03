using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BRail.Nis.ServiceImport.Framework.Factory
{
    public class XsdDataContractImporterFactory : IXsdDataContractImporterFactory
    {
        public XsdDataContractImporter Create(CodeDomProvider codeDomProvider, CodeCompileUnit codeCompileUnit, IDictionary<string, string> namespaceMappings)
        {
            var importOptions = new ImportOptions
                {
                    CodeProvider = codeDomProvider,
                    EnableDataBinding = false
                };
            foreach (var namespaceEntry in namespaceMappings)
                importOptions.Namespaces.Add(namespaceEntry.Key, namespaceEntry.Value);
            importOptions.ReferencedCollectionTypes.Add(typeof(List<>));
            return new XsdDataContractImporter(codeCompileUnit) { Options = importOptions };
        }
    }
}
