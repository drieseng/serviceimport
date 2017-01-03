using System;
using System.CodeDom;

namespace BRail.Nis.ServiceImport.Framework.CodeDom
{
    internal class CodeTypeName
    {
        public CodeTypeName(string @namespace, string type)
        {
            if (@namespace == null)
                throw new ArgumentNullException(nameof(@namespace));
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (type.Length == 0)
                throw new ArgumentException("Cannot be a zero-length string.", nameof(type));

            Namespace = @namespace;
            Type = type;
        }

        public string Namespace
        {
            get; }

        public string Type
        {
            get; }

        public override string ToString()
        {
            if (Namespace.Length == 0)
                return Type;

            return string.Join(".", Namespace, Type);
        }

        public static implicit operator CodeTypeName(string name)
        {
            var lastSeparator = name.LastIndexOf('.');
            if (lastSeparator == -1)
                return new CodeTypeName(string.Empty, name);

            var @namespace = name.Substring(0, lastSeparator);
            var type = name.Substring(lastSeparator + 1);
            return new CodeTypeName(@namespace, type);
        }

        public CodeTypeDeclaration Find(CodeCompileUnit compileUnit)
        {
            foreach (CodeNamespace codeNamespace in compileUnit.Namespaces)
            {
                if (codeNamespace.Name != Namespace)
                    continue;

                foreach (CodeTypeDeclaration typeDeclaration in codeNamespace.Types)
                {
                    if (typeDeclaration.Name == Type)
                        return typeDeclaration;
                }

                break;
            }

            return null;
        }
    }
}
