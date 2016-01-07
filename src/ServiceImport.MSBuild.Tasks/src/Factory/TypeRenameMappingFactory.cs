using System;
using BRail.Nis.ServiceImport.MSBuild.Tasks.Model;
using Microsoft.Build.Framework;

namespace BRail.Nis.ServiceImport.MSBuild.Tasks.Factory
{
    internal class TypeRenameMappingFactory
    {
        public TypeRenameMapping Create(ITaskItem taskItem)
        {
            if (taskItem == null)
                throw new ArgumentNullException("taskItem");

            var originalTypeName = taskItem.ItemSpec;

            var newTypeName = taskItem.GetMetadata("NewTypeName");
            if (newTypeName == null)
                throw new Exception("NewTypeName cannot be null.");

            return new TypeRenameMapping
                {
                    OriginalTypeName = originalTypeName,
                    NewTypeName = newTypeName
                };
        }
    }
}
