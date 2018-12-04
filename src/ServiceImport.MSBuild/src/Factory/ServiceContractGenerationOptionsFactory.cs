using System;
using System.ServiceModel.Description;
using Microsoft.Build.Framework;

namespace ServiceImport.MSBuild.Factory
{
    public class ServiceContractGenerationOptionsFactory
    {
        public ServiceContractGenerationOptions Create(ITaskItem taskItem)
        {
            if (taskItem == null)
                throw new ArgumentNullException(nameof(taskItem));

            var options = ServiceContractGenerationOptions.None;

            if (GetBooleanMetadataValue(taskItem, MetadataNames.ClientClass))
            {
                options |= ServiceContractGenerationOptions.ClientClass;
            }

            if (GetBooleanMetadataValue(taskItem, MetadataNames.AsynchronousMethods))
            {
                options |= ServiceContractGenerationOptions.AsynchronousMethods;
            }

            if (GetBooleanMetadataValue(taskItem, MetadataNames.ChannelInterface))
            {
                options |= ServiceContractGenerationOptions.ChannelInterface;
            }

            if (GetBooleanMetadataValue(taskItem, MetadataNames.EventBasedAsynchronousMethods))
            {
                options |= ServiceContractGenerationOptions.EventBasedAsynchronousMethods;
            }

            if (GetBooleanMetadataValue(taskItem, MetadataNames.InternalTypes))
            {
                options |= ServiceContractGenerationOptions.InternalTypes;
            }

            if (GetBooleanMetadataValue(taskItem, MetadataNames.TaskBasedAsynchronousMethod))
            {
                options |= ServiceContractGenerationOptions.TaskBasedAsynchronousMethod;
            }

            if (GetBooleanMetadataValue(taskItem, MetadataNames.TypedMessages))
            {
                options |= ServiceContractGenerationOptions.TypedMessages;
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
            public const string ClientClass = "ClientClass";
            public const string AsynchronousMethods = "AsynchronousMethods";
            public const string ChannelInterface = "ChannelInterface";
            public const string EventBasedAsynchronousMethods = "EventBasedAsynchronousMethods";
            public const string InternalTypes = "InternalTypes";
            public const string TaskBasedAsynchronousMethod = "TaskBasedAsynchronousMethod";
            public const string TypedMessages = "TypedMessages";
        }
    }
}
