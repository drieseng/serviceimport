﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel.Description;
using System.Xml.Schema;
using ServiceImport.Framework.CodeDom;

namespace ServiceImport.Framework.Extension
{
    /// <summary>
    /// Supports changing the type name and/or namespace of generated type declarations.
    /// </summary>
    /// <remarks>
    /// This is not a WCF extension because the client class is not yet available when <see cref="IServiceContractGenerationExtension.GenerateContract(ServiceContractGenerationContext)"/>
    /// is invoked.
    /// </remarks>
    public class TypeRenameExtension : IXsdImportExtension
    {
        private readonly IDictionary<string, string> _typeRenames;

        public TypeRenameExtension(IDictionary<string, string> typeRenames)
        {
            if (typeRenames == null)
                throw new ArgumentNullException(nameof(typeRenames));

            _typeRenames = typeRenames;
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

            foreach (var typeRenameEntry in _typeRenames)
            {
                var originalTypeName = (CodeTypeName) typeRenameEntry.Key;
                var newTypeName = (CodeTypeName) typeRenameEntry.Value;

                var typeDeclaration = compileUnit.FindTypeDeclaration(originalTypeName);
                if (typeDeclaration == null)
                    throw new Exception($"Type '{originalTypeName}' does not exist.");

                // lookup the original namespace
                var originalNamespace = compileUnit.Namespaces().Single(n => n.Name == originalTypeName.Namespace);

                // if both the original and new type name are in the same namespace, then we need to
                // ensure the new type name doesn't already exist in the original namespace
                if (originalTypeName.Namespace == newTypeName.Namespace)
                {
                    if (originalNamespace.Types().SingleOrDefault(t => t.Name == newTypeName.Type) != null)
                        throw new Exception($"Type '{newTypeName}' already exists.");
                }
                else
                {
                    var newNamespace = compileUnit.Namespaces().SingleOrDefault(n => n.Name == newTypeName.Namespace);
                    if (newNamespace == null)
                    {
                        newNamespace = new CodeNamespace(newTypeName.Namespace);
                        compileUnit.Namespaces.Add(newNamespace);
                    }
                    else
                    {
                        if (newNamespace.Types().Any(p => p.Name == newTypeName.Type))
                            throw new Exception($"Type '{newTypeName}' already exists.");
                    }

                    // add the type to the new namespace
                    newNamespace.Types.Add(typeDeclaration);

                    // remove the type from the original namespace
                    originalNamespace.Types.Remove(typeDeclaration);

                    // remove the original namespace if we just removed the last type from it
                    if (originalNamespace.Types.Count == 0)
                        compileUnit.Namespaces.Remove(originalNamespace);
                }

                // modify the name of the type
                typeDeclaration.Name = newTypeName.Type;

                var originalFullTypeName = originalTypeName.ToString();
                var newFullTypeName = newTypeName.ToString();

                // Updating a type declaration in CodeDOM does not update type parameters of that type
                foreach (var ns in compileUnit.Namespaces())
                {
                    foreach (CodeTypeDeclaration type in ns.Types)
                    {
                        foreach (CodeTypeMember member in type.Members)
                        {
                            if (member is CodeMemberField field)
                            {
                                if (field.Type.BaseType == originalFullTypeName)
                                {
                                    field.Type.BaseType = newFullTypeName;
                                }
                                else
                                {
                                    foreach (CodeTypeReference typeArgument in field.Type.TypeArguments)
                                    {
                                        if (typeArgument.BaseType == originalFullTypeName)
                                        {
                                            typeArgument.BaseType = newFullTypeName;
                                        }
                                    }
                                }

                                continue;
                            }

                            if (member is CodeMemberProperty property)
                            {
                                if (property.Type.BaseType == originalFullTypeName)
                                {
                                    property.Type.BaseType = newFullTypeName;
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
