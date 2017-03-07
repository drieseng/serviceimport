using System.CodeDom;

namespace ServiceImport.Framework.Extension
{
    public interface IDataContractGenerationExtension
    {
        /// <summary>
        /// Called after the contract has been generated.
        /// </summary>
        /// <param name="compileUnit">The compile unit.</param>
        void GenerateContract(CodeCompileUnit compileUnit);
    }
}
