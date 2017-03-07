using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel.Description;
using System.Xml.Schema;
using ServiceImport.Framework.CodeDom;

namespace ServiceImport.Framework.Extension
{
    /// <summary>
    /// Supports changing the access modifier of generated type declarations.
    /// </summary>
    /// <remarks>
    /// This is not a WCF extension because the client class is not yet available when <see cref="IServiceContractGenerationExtension.GenerateContract(ServiceContractGenerationContext)"/>
    /// is invoked.
    /// </remarks>
    public class TypeAccessModifierExtension : IXsdImportExtension
    {
        private readonly IDictionary<string, TypeAccessModifier> _typeAccessModifiers;

        public TypeAccessModifierExtension(IDictionary<string, TypeAccessModifier> typeAccessModifiers)
        {
            if (typeAccessModifiers == null)
                throw new ArgumentNullException(nameof(typeAccessModifiers));

            _typeAccessModifiers = typeAccessModifiers;
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

            foreach (var typeAccessModifierEntry in _typeAccessModifiers)
            {
                var typeName = typeAccessModifierEntry.Key;
                var accessModifier = typeAccessModifierEntry.Value;

                var typeDeclaration = compileUnit.FindTypeDeclaration(typeName);
                if (typeDeclaration == null)
                    throw new Exception($"Type '{typeName}' does not exist.");

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
                        throw new Exception($"Access modifier '{accessModifier}' is not supported.");
                }
            }
        }

        #endregion IXsdImportExtension implementation
    }
}
