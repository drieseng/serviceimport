using System;
using Microsoft.Build.Framework;
using ServiceImport.MSBuild.Model;

namespace ServiceImport.MSBuild.Factory
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
