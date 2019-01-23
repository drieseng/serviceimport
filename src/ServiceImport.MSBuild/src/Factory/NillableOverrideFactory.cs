using System;
using System.Xml;
using Microsoft.Build.Framework;
using ServiceImport.Framework.Model;

namespace ServiceImport.MSBuild.Factory
{
    public class NillableOverrideFactory
    {
        private readonly XmlQualifiedNameFactory _xmlQualifiedNameFactory;

        public NillableOverrideFactory(XmlQualifiedNameFactory xmlQualifiedNameFactory)
        {
            _xmlQualifiedNameFactory = xmlQualifiedNameFactory;
        }

        public NillableOverride Create(ITaskItem taskItem)
        {
            if (taskItem == null)
                throw new ArgumentNullException(nameof(taskItem));

            var complexTypeQualifiedName = taskItem.ItemSpec;
            if (complexTypeQualifiedName == null)
            {
                throw new ArgumentException(
                    $"The 'Include' attribute of the 'OverrideNillable' item group must not be null.");
            }

            var element = taskItem.GetMetadata(MetadataNames.Element);
            if (element == null)
            {
                throw new ArgumentException($"The '{MetadataNames.Element}' element of the 'OverrideNillable' item group should be specified.");
            }

            return new NillableOverride
                {
                    ComplexTypeName = _xmlQualifiedNameFactory.Create(complexTypeQualifiedName),
                    ElementName = element,
                    IsNillable = GetBooleanMetadataValue(taskItem, MetadataNames.IsNillable)
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
            public const string Element = "Element";
            public const string IsNillable = "IsNillable";
        }
    }
}
