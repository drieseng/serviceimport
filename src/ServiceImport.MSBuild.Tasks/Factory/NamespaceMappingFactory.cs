using System;
using BRail.Nis.ServiceImport.MSBuild.Tasks.Model;
using Microsoft.Build.Framework;

namespace BRail.Nis.ServiceImport.MSBuild.Tasks.Factory
{
    internal class NamespaceMappingFactory
    {
        public NamespaceMapping Create(ITaskItem taskItem)
        {
            if (taskItem == null)
                throw new ArgumentNullException("taskItem");

            var targetNamespace = taskItem.ItemSpec;

            var clrNamespace = taskItem.GetMetadata("Namespace");
            if (clrNamespace == null)
                throw new Exception("Namespace cannot be null.");

            return new NamespaceMapping
                {
                    ClrNamespace = clrNamespace,
                    TargetNamespace = targetNamespace
                };
        }
    }
}
