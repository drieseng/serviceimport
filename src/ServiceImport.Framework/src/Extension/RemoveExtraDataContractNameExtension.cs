using System;
using ServiceImport.Framework.CodeDom;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel.Description;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Schema;

namespace ServiceImport.Framework.Extension
{
    /// <summary>
    /// Removes the <see cref="DataContractAttribute.Name"/> of a <see cref="DataContractAttribute"/> that is applied
    /// to a <see cref="Type"/> when its value matching the name of the <see cref="Type"/>.
    /// </summary>
    public class RemoveExtraDataContractNameExtension : IXsdImportExtension
    {
        #region IXsdImportExtension implementation

        void IXsdImportExtension.BeforeImport(XmlSchemaSet xmlSchemas)
        {
        }

        void IXsdImportExtension.ImportContract(XsdDataContractImporter importer)
        {
        }

        #endregion IXsdImportExtension implementation

        #region IDataContractGenerationExtension implementation

        void IDataContractGenerationExtension.GenerateContract(CodeCompileUnit compileUnit)
        {
            foreach (var ns in compileUnit.Namespaces())
            {
                foreach (var typeDeclaration in ns.Types())
                {
                    var dataContractAttribute = typeDeclaration.GetDataContractAttribute();
                    if (dataContractAttribute != null)
                    {
                        var dataContractNameArgument = dataContractAttribute.FindArgumentByName("Name");
                        if (dataContractNameArgument != null)
                        {
                            var dataContractName = dataContractNameArgument.GetStringValue();
                            if (dataContractName != null && dataContractName == typeDeclaration.Name)
                            {
                                dataContractAttribute.Arguments.Remove(dataContractNameArgument);
                            }
                        }
                    }
                }
            }
        }

        #endregion IDataContractGenerationExtension implementation

    }
}
