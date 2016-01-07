using System;
using BRail.Nis.ServiceImport.Framework.CodeDom;
using BRail.Nis.ServiceImport.MSBuild.Tasks.Model;
using Microsoft.Build.Framework;

namespace BRail.Nis.ServiceImport.MSBuild.Tasks.Factory
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
