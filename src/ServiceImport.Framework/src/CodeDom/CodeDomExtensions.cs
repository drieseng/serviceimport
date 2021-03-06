﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ServiceImport.Framework.CodeDom
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

        public static CodeAttributeDeclaration GetDataContractAttribute(this CodeTypeMember typeMember)
        {
            return typeMember.CustomAttributes().SingleOrDefault(p => p.AttributeType.BaseType == typeof(DataContractAttribute).FullName);
        }

        public static string GetStringValue(this CodeAttributeArgument attributeArgument)
        {
            if (attributeArgument.Value == null)
            {
                throw new ArgumentException(
                    $"The value of the {attributeArgument.Name} argument cannot be null.",
                    nameof(attributeArgument));
            }

            if (!(attributeArgument.Value is CodePrimitiveExpression primitiveExpression))
            {
                throw new ArgumentException(
                    $"The value of the {attributeArgument.Name} argument should be a primitive expression.",
                    nameof(attributeArgument));
            }

            var primitiveValue = primitiveExpression.Value;
            if (primitiveValue == null)
            {
                throw new ArgumentException(
                    $"The primitive value of the {attributeArgument.Name} argument cannot be null.",
                    nameof(attributeArgument));
            }

            if (!(primitiveValue is string name))
            {
                throw new ArgumentException(
                    $"The primitive value of the {attributeArgument.Name} argument should be of type '{typeof(string).FullName}'.",
                    nameof(attributeArgument));
            }

            return name;
        }

        public static IEnumerable<CodeAttributeDeclaration> CustomAttributes(this CodeTypeMember typeMember)
        {
            return typeMember.CustomAttributes.Cast<CodeAttributeDeclaration>();
        }

        public static IEnumerable<CodeTypeDeclaration> Types(this CodeNamespace @namespace)
        {
            return @namespace.Types.Cast<CodeTypeDeclaration>();
        }

        public static IEnumerable<CodeNamespace> Namespaces(this CodeCompileUnit compileUnit)
        {
            return compileUnit.Namespaces.Cast<CodeNamespace>();
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

        public static CodeTypeMember FindMember(this CodeTypeDeclaration typeDeclaration, string memberName)
        {
            foreach (CodeTypeMember member in typeDeclaration.Members)
            {
                if (member.Name == memberName)
                {
                    return member;
                }
            }

            return null;
        }

            public static CodeTypeDeclaration FindTypeDeclaration(this CodeCompileUnit compileUnit, CodeTypeName typeName)
        {
            foreach (CodeNamespace ns in compileUnit.Namespaces)
            {
                if (ns.Name != typeName.Namespace)
                    continue;

                var nameParts = typeName.Type.Split('+');
                foreach (CodeTypeDeclaration type in ns.Types)
                {
                    if (type.Name != nameParts[0])
                    {
                        continue;
                    }

                    if (nameParts.Length == 1)
                    {
                        return type;
                    }

                    var namesQueue = new Queue<string>(nameParts);
                    namesQueue.Dequeue();

                    var parentType = type;

                    while (namesQueue.Count > 0)
                    {
                        var childTypeName = namesQueue.Dequeue();

                        var nestedType = parentType.FindNestedTypeDeclaration(childTypeName);
                        if (nestedType == null)
                        {
                            return null;
                        }

                        parentType = nestedType;
                    }

                    return parentType;
                }

                foreach (CodeTypeDeclaration type in ns.Types)
                {
                    if (type.Name == typeName.Type)
                        return type;
                }
            }

            return null;
        }

        public static CodeAttributeArgument FindArgumentByName(this CodeAttributeArgumentCollection arguments, string name)
        {
            foreach (CodeAttributeArgument argument in arguments)
            {
                if (argument.Name == name)
                {
                    return argument;
                }
            }

            return null;
        }

        public static CodeTypeDeclaration FindNestedTypeDeclaration(this CodeTypeDeclaration typeDeclaration, string typeName)
        {
            foreach (CodeTypeMember typeMember in typeDeclaration.Members)
            {
                if (typeMember is CodeTypeDeclaration nestedType && nestedType.Name == typeName)
                {
                    return nestedType;
                }
            }

            return null;
        }


        public static CodeAttributeArgument FindArgumentByName(this CodeAttributeDeclaration attributeDeclaration, string name)
        {
            foreach (CodeAttributeArgument argument in attributeDeclaration.Arguments)
            {
                if (argument.Name == name)
                {
                    return argument;
                }
            }

            return null;
        }

        public static bool IsPascalCase(this string text)
        {
            if (text.Length == 0)
                throw new ArgumentException("Cannot be a zero-length string.", nameof(text));

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
