using System;
using System.CodeDom;
using System.Collections.Generic;

namespace BRail.Nis.ServiceImport.Framework.CodeDom
{
    internal static class CodeDomExtensions
    {
        public static CodeNamespace Clone(this CodeNamespace ns)
        {
            var clone = new CodeNamespace(ns.Name);
            foreach (CodeNamespaceImport import in ns.Imports)
                clone.Imports.Add(import);
            return clone;
        }

        public static IEnumerable<CodeTypeDeclaration> Types(this CodeCompileUnit compileUnit)
        {
            foreach (CodeNamespace ns in compileUnit.Namespaces)
            {
                foreach (CodeTypeDeclaration type in ns.Types)
                {
                    yield return type;
                }
            }
        }

        public static IEnumerable<CodeMemberField> Fields(this CodeTypeDeclaration typeDeclaration)
        {
            foreach (var member in typeDeclaration.Members)
            {
                var field = member as CodeMemberField;
                if (field == null)
                    continue;

                yield return field;
            }
        }

        public static IEnumerable<CodeMemberProperty> Properties(this CodeTypeDeclaration typeDeclaration)
        {
            foreach (var member in typeDeclaration.Members)
            {
                var property = member as CodeMemberProperty;
                if (property == null)
                    continue;

                yield return property;
            }
        }

        public static CodeTypeReference ToNullable(this CodeTypeReference typeReference)
        {
            var nullable = new CodeTypeReference(typeof(Nullable<>));
            nullable.TypeArguments.Add(typeReference);
            return nullable;
        }

        public static CodeTypeDeclaration FindTypeDeclaration(this CodeCompileUnit compileUnit, CodeTypeName typeName)
        {
            foreach (CodeNamespace ns in compileUnit.Namespaces)
            {
                if (ns.Name != typeName.Namespace)
                    continue;

                foreach (CodeTypeDeclaration type in ns.Types)
                {
                    if (type.Name == typeName.Type)
                        return type;
                }
            }

            return null;
        }

        public static bool IsPascalCase(this string text)
        {
            if (text.Length == 0)
                throw new ArgumentException("Cannot be a zero-length string.", "text");

            if (text.Length == 1)
                return char.IsLower(text[0]);

            if (char.IsLower(text[0]))
                return true;

            // if both the first and second character are upper case, then we consider it to be an abbreviation
            // and as such allow it to be upper case
            if (!char.IsLower(text[1]))
                return true;

            return false;
        }

        public static string ToPascalCase(this string text)
        {
            if (IsPascalCase(text))
                return text;

            if (text[0] == '_')
                return text.Substring(0, 1) + text.Substring(1, 1).ToLower() + text.Substring(2);

            return text.Substring(0, 1).ToLower() + text.Substring(1);
        }
    }
}
