using Microsoft.Build.Framework;

namespace ServiceImport.MSBuild.Extension
{
    public static class TaskItemExtensions
    {
        public static string[] ToStringArray(this ITaskItem[] taskItems)
        {
            var itemSpecs = new string[taskItems.Length];
            for (var i = 0; i < taskItems.Length; i++)
            {
                var taskItem = taskItems[i];
                itemSpecs[i] = taskItem.ItemSpec;
            }

            return itemSpecs;
        }

    }
}
