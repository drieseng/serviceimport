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

            var clientClass = taskItem.GetMetadata("ClientClass");
            if (clientClass != null && bool.Parse(clientClass))
                options |= ServiceContractGenerationOptions.ClientClass;

            var asynchronousMethods = taskItem.GetMetadata("AsynchronousMethods");
            if (asynchronousMethods != null && bool.Parse(asynchronousMethods))
                options |= ServiceContractGenerationOptions.AsynchronousMethods;

            var channelInterface = taskItem.GetMetadata("ChannelInterface");
            if (channelInterface != null && bool.Parse(channelInterface))
                options |= ServiceContractGenerationOptions.ChannelInterface;

            var eventBasedAsynchronousMethods = taskItem.GetMetadata("EventBasedAsynchronousMethods");
            if (eventBasedAsynchronousMethods != null && bool.Parse(eventBasedAsynchronousMethods))
                options |= ServiceContractGenerationOptions.EventBasedAsynchronousMethods;

            var internalTypes = taskItem.GetMetadata("InternalTypes");
            if (internalTypes != null && bool.Parse(internalTypes))
                options |= ServiceContractGenerationOptions.InternalTypes;

            var taskBasedAsynchronousMethod = taskItem.GetMetadata("TaskBasedAsynchronousMethod");
            if (taskBasedAsynchronousMethod != null && bool.Parse(taskBasedAsynchronousMethod))
                options |= ServiceContractGenerationOptions.TaskBasedAsynchronousMethod;

            var typedMessages = taskItem.GetMetadata("TypedMessages");
            if (typedMessages != null && bool.Parse(typedMessages))
                options |= ServiceContractGenerationOptions.TypedMessages;

            return options;
        }
    }
}
