using System;
using System.CodeDom;
using System.Xml.Schema;
using Microsoft.Build.Framework;
using ServiceImport.Framework.Model;

namespace ServiceImport.MSBuild.Factory
{
    internal class XmlTypeMappingFactory
    {
        private const string CodeTypeReferenceMetadataName = "CodeTypeReference";
        private const string IsStructMetadataName = "IsStruct";

        public XmlTypeMapping Create(ITaskItem taskItem)
        {
            if (taskItem == null)
                throw new ArgumentNullException(nameof(taskItem));

            if (!Enum.TryParse<XmlTypeCode>(taskItem.ItemSpec, false, out var xmlTypeCode))
            {
                throw new Exception($"'Value '{taskItem.ItemSpec}' is not a valid {typeof(XmlTypeCode).Name}.");
            }

            var codeTypeReference = taskItem.GetMetadata(CodeTypeReferenceMetadataName);
            if (codeTypeReference == null)
            {
                throw new Exception($"'{CodeTypeReferenceMetadataName}' metadata cannot be null.");
            }

            var isStructLiteral = taskItem.GetMetadata(IsStructMetadataName);
            if (isStructLiteral == null)
            {
                throw new Exception($"'{IsStructMetadataName}' cannot be null.");
            }

            if (!bool.TryParse(isStructLiteral, out var isStruct))
            {
                throw new Exception($"'Value '{isStructLiteral}' for '{IsStructMetadataName}' is not a valid boolean.");
            }

            return new XmlTypeMapping
                {
                    IsStruct = isStruct,
                    XmlTypeCode = xmlTypeCode,
                    CodeTypeReference = new CodeTypeReference(codeTypeReference),
                };
        }
    }
}
