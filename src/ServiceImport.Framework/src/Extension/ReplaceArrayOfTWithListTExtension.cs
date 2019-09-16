using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Schema;

namespace ServiceImport.Framework.Extension
{
    /// <summary>
    /// Replaces references to ArrayOfX classes that derive from List&lt;T&gt; with List&lt;T&gt;.
    /// </summary>
    /// <remarks>
    /// The WCF tooling do not generate List&lt;T&gt; when T is defined as a simple type.
    /// </remarks>
    public class ReplaceArrayOfTWithListTExtension : IXsdImportExtension
    {
        #region IXsdImportExtension implementation

        void IXsdImportExtension.BeforeImport(XmlSchemaSet xmlSchemas)
        {
        }

        void IXsdImportExtension.ImportContract(XsdDataContractImporter importer)
        {
        }

        void IDataContractGenerationExtension.GenerateContract(CodeCompileUnit compileUnit)
        {
            if (compileUnit == null)
                throw new ArgumentNullException(nameof(compileUnit));

            var referencesToReplace = new Dictionary<CodeTypeReference, CodeTypeReference>();

            foreach (CodeNamespace ns in compileUnit.Namespaces)
            {
                for (var i = (ns.Types.Count - 1); i >= 0; i--)
                {
                    var typeDeclaration = ns.Types[i];

                    if (!typeDeclaration.Name.StartsWith("ArrayOf"))
                        continue;

                    if (typeDeclaration.BaseTypes.Count != 1)
                        continue;

                    new CodeTypeReference().BaseType = ns.Name + "." + typeDeclaration.Name;

                    var baseType = typeDeclaration.BaseTypes[0];

                    if (baseType.BaseType != typeof(List<>).FullName || baseType.TypeArguments.Count != 1)
                        continue;

                    var referenceToReplace = new CodeTypeReference
                        {
                            BaseType = ns.Name + "." + typeDeclaration.Name
                        };

                    referencesToReplace.Add(referenceToReplace, baseType);

                    ns.Types.RemoveAt(i);
                }
            }

            foreach (var replace in referencesToReplace)
            {
                var typeToReplace = replace.Key;
                var replacingType = replace.Value;

                foreach (CodeNamespace ns in compileUnit.Namespaces)
                {
                    foreach (CodeTypeDeclaration typeDeclaration in ns.Types)
                    {
                        foreach (CodeTypeMember member in typeDeclaration.Members)
                        {
                            if (member is CodeMemberField field)
                            {
                                if (field.Type.BaseType == typeToReplace.BaseType)
                                {
                                    field.Type = replacingType;
                                }
                            }
                            else if (member is CodeMemberProperty property)
                            {
                                if (property.Type.BaseType == typeToReplace.BaseType)
                                {
                                    property.Type = replacingType;
                                }
                            }
                            else if (member is CodeMemberMethod method)
                            {
                                foreach (CodeParameterDeclarationExpression param in method.Parameters)
                                {
                                    if (param.Type.BaseType == typeToReplace.BaseType)
                                    {
                                        param.Type = replacingType;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion IXsdImportExtension implementation
    }
}
