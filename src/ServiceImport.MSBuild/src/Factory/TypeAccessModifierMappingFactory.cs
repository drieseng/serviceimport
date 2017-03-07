using System;
using Microsoft.Build.Framework;
using ServiceImport.Framework.CodeDom;
using ServiceImport.MSBuild.Model;

namespace ServiceImport.MSBuild.Factory
{
    internal class TypeAccessModifierMappingFactory
    {
        public TypeAccessModifierMapping Create(ITaskItem taskItem)
        {
            if (taskItem == null)
                throw new ArgumentNullException("taskItem");

            var typeName = taskItem.ItemSpec;

            var accessModifier = taskItem.GetMetadata("AccessModifier");
            if (accessModifier == null)
                throw new Exception("AccessModifier cannot be null.");

            return new TypeAccessModifierMapping
            {
                TypeName = typeName,
                AccessModifier = (TypeAccessModifier) Enum.Parse(typeof (TypeAccessModifier), accessModifier)
            };
        }
    }
}
