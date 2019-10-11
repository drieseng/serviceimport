using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel.Description;
using System.Xml.Schema;
using ServiceImport.Framework.CodeDom;
using ServiceImport.Framework.Helper;
using ServiceImport.Framework.Model;

namespace ServiceImport.Framework.Extension
{
    /// <summary>
    /// Supports changing the type name and/or namespace of generated type declarations.
    /// </summary>
    /// <remarks>
    /// This is not a WCF extension because the client class is not yet available when <see cref="IServiceContractGenerationExtension.GenerateContract(ServiceContractGenerationContext)"/>
    /// is invoked.
    /// </remarks>
    public class IsRequiredMemberOverrideExtension : IXsdImportExtension
    {
        private readonly List<IsRequiredMemberOverride> _requiredMemberOverrides;

        public IsRequiredMemberOverrideExtension(List<IsRequiredMemberOverride> requiredMemberOverrides)
        {
            if (requiredMemberOverrides == null)
                throw new ArgumentNullException(nameof(requiredMemberOverrides));

            _requiredMemberOverrides = requiredMemberOverrides;
        }

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

            foreach (var requiredMemberOverride in _requiredMemberOverrides)
            {
                var typeDeclaration = compileUnit.FindTypeDeclaration((CodeTypeName) requiredMemberOverride.Type);
                if (typeDeclaration == null)
                    throw new Exception($"Type '{requiredMemberOverride.Type}' does not exist.");

                var member = typeDeclaration.FindMember(requiredMemberOverride.Member);
                if (member == null)
                    throw new Exception($"Member '{requiredMemberOverride.Member}' does not exist in type '{requiredMemberOverride.Type}'.");

                var dataMemberAttribute = member.CustomAttributes.SingleOrDefault<CodeAttributeDeclaration>(p => p.Name == typeof(DataMemberAttribute).FullName);
                if (dataMemberAttribute == null)
                {
                    dataMemberAttribute = new CodeAttributeDeclaration(new CodeTypeReference(typeof(DataMemberAttribute)));
                }

                var isRequiredArgument = dataMemberAttribute.Arguments.FindArgumentByName(nameof(DataMemberAttribute.IsRequired));
                if (isRequiredArgument == null)
                {
                    dataMemberAttribute.Arguments.Add(new CodeAttributeArgument(
                        nameof(DataMemberAttribute.IsRequired),
                        new CodePrimitiveExpression(requiredMemberOverride.IsRequired)));
                }
                else
                {
                    isRequiredArgument.Value = new CodePrimitiveExpression(requiredMemberOverride.IsRequired);
                }
            }
        }

        #endregion IXsdImportExtension implementation
    }
}
