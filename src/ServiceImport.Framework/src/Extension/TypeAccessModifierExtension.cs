using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Reflection;
using BRail.Nis.ServiceImport.Framework.CodeDom;

namespace BRail.Nis.ServiceImport.Framework.Extension
{
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
                {
                    //throw new Exception(string.Format("Type '{0}' does not exist.", typeName));
                    return;
                }

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
