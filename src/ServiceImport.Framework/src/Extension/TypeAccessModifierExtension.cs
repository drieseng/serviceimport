using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Reflection;
using System.ServiceModel.Description;
using BRail.Nis.ServiceImport.Framework.CodeDom;

namespace BRail.Nis.ServiceImport.Framework.Extension
{
    /// <summary>
    /// Supports changing the access modifier of generated type declarations.
    /// </summary>
    /// <remarks>
    /// This is not a WCF extension because the client class of not yet available when <see cref="IServiceContractGenerationExtension.GenerateContract(ServiceContractGenerationContext)"/>
    /// is invoked.
    /// </remarks>
    public class TypeAccessModifierExtension
    {
        public void Apply(IDictionary<string, TypeAccessModifier> typeAccessModifiers, CodeCompileUnit codeCompileUnit)
        {
            foreach (var typeAccessModifierEntry in typeAccessModifiers)
            {
                var typeName = typeAccessModifierEntry.Key;
                var accessModifier = typeAccessModifierEntry.Value;

                var typeDeclaration = codeCompileUnit.FindTypeDeclaration(typeName);
                if (typeDeclaration == null)
                    throw new Exception(string.Format("Type '{0}' does not exist.", typeName));

                switch (accessModifier)
                {
                    case TypeAccessModifier.Internal:
                        typeDeclaration.TypeAttributes &= ~TypeAttributes.VisibilityMask;
                        typeDeclaration.TypeAttributes &= TypeAttributes.NotPublic;
                        break;
                    case TypeAccessModifier.Public:
                        typeDeclaration.TypeAttributes &= ~TypeAttributes.VisibilityMask;
                        typeDeclaration.TypeAttributes &= TypeAttributes.NotPublic;
                        break;
                    default:
                        throw new Exception(string.Format("Access modifier '{0}' is not supported.", accessModifier));
                }
            }
        }

    }
}
