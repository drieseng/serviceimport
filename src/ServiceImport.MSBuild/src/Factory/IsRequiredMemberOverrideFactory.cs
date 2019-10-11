using System;
using Microsoft.Build.Framework;
using ServiceImport.Framework.Model;

namespace ServiceImport.MSBuild.Factory
{
    public class IsRequiredMemberOverrideFactory
    {
        public IsRequiredMemberOverride Create(ITaskItem taskItem)
        {
            if (taskItem == null)
                throw new ArgumentNullException(nameof(taskItem));

            var typeName = taskItem.ItemSpec;
            if (typeName == null)
            {
                throw new ArgumentException($"The 'Include' attribute of the '{nameof(IsRequiredMemberOverride)}' item group must not be null.");
            }

            var memberName = taskItem.GetMetadata(MetadataNames.Member);
            if (memberName == null)
            {
                throw new ArgumentException($"The '{MetadataNames.Member}' element of the '{nameof(IsRequiredMemberOverride)}' item group should be specified.");
            }

            return new IsRequiredMemberOverride
                {
                    Type = typeName,
                    Member = memberName,
                    IsRequired = GetBooleanMetadataValue(taskItem, MetadataNames.IsRequired)
                };
        }

        private static bool GetBooleanMetadataValue(ITaskItem taskItem, string metadataName)
        {
            var metadataValue = taskItem.GetMetadata(metadataName);
            if (metadataValue == null)
            {
                throw new ArgumentException($"The {metadataName} metadata is not defined.");
            }

            if (metadataValue == null)
            {
                throw new ArgumentException($"The value of the {metadataName} metadata is a zero-length string.");
            }

            if (!bool.TryParse(metadataValue, out var flag))
            {
                throw new ArgumentException($"The value of the {metadataName} metadata is not a valid Boolean.");
            }

            return flag;
        }

        private static class MetadataNames
        {
            public const string Member = "Member";
            public const string IsRequired = "IsRequired";
        }
    }
}
