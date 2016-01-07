﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using BRail.Nis.ServiceImport.Framework.CodeDom;

namespace BRail.Nis.ServiceImport.Framework.Extension
{
    public class TypeRenameExtension
    {
        public void Apply(IDictionary<string, string> typeRenames, CodeCompileUnit codeCompileUnit)
        {
            foreach (var typeRenameEntry in typeRenames)
            {
                var originalTypeName = (CodeTypeName) typeRenameEntry.Key;
                var newTypeName = (CodeTypeName) typeRenameEntry.Value;

                var typeDeclaration = codeCompileUnit.FindTypeDeclaration(originalTypeName);
                if (typeDeclaration == null)
                    throw new Exception();

                // lookup the original namespace
                var originalNamespace = codeCompileUnit.Namespaces().Single(n => n.Name == originalTypeName.Namespace);

                // if both the original and new type name are in the same namespace, then we need to
                // ensure the new type name doesn't already exist in the original namespace
                if (originalTypeName.Namespace == newTypeName.Namespace)
                {
                    if (originalNamespace.Types().SingleOrDefault(t => t.Name == newTypeName.Type) != null)
                        throw new Exception("Type already exists.");

                    // modify the name of the type
                    typeDeclaration.Name = newTypeName.Type;

                    continue;
                }

                var newNamespace = codeCompileUnit.Namespaces().SingleOrDefault(n => n.Name == newTypeName.Namespace);
                if (newNamespace == null)
                {
                    newNamespace = new CodeNamespace(newTypeName.Namespace);
                    codeCompileUnit.Namespaces.Add(newNamespace);
                }
                else
                {
                    if (newNamespace.Types().Any(p => p.Name == newTypeName.Type))
                        throw new Exception("Type already exists.");
                }

                // modify the name of the type
                typeDeclaration.Name = newTypeName.Type;

                // add the type to the new namespace
                newNamespace.Types.Add(typeDeclaration);

                // remove the type from the original namespace
                originalNamespace.Types.Remove(typeDeclaration);

                // remove the original namespace if we just removed the last type from it
                if (originalNamespace.Types.Count == 0)
                    codeCompileUnit.Namespaces.Remove(originalNamespace);
            }
        }
    }
}
