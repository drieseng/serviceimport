using System;
using System.ServiceModel.Description;
using Microsoft.Build.Framework;
using ServiceImport.Framework;

namespace ServiceImport.MSBuild.Factory
{
    public class DataContractGenerationOptionsFactory
    {
        public DataContractGenerationOptions Create(ITaskItem taskItem)
        {
            if (taskItem == null)
                throw new ArgumentNullException(nameof(taskItem));

            var options = DataContractGenerationOptions.Default;

            if (GetBooleanMetadataValue(taskItem, MetadataNames.OptionalElementAsNullable))
            {
                options |= DataContractGenerationOptions.OptionalElementAsNullable;
            }

            return options;
        }

        private static bool GetBooleanMetadataValue(ITaskItem taskItem, string metadataName)
        {
            var metadataValue = taskItem.GetMetadata(metadataName);
            if (string.IsNullOrEmpty(metadataValue))
            {
                return false;
            }

            if (!bool.TryParse(metadataValue, out var flag))
            {
                throw new ArgumentException($"The value of the {metadataName} metadata is not a valid Boolean.");
            }

            return flag;
        }

        private static class MetadataNames
        {
            public const string OptionalElementAsNullable = "OptionalElementAsNullable";
        }
    }
}
