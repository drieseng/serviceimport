using System;
using System.CodeDom;
using System.Xml.Schema;
using Microsoft.Build.Framework;
using ServiceImport.MSBuild.Model;

namespace ServiceImport.MSBuild.Factory
{
    internal class XmlTypeMappingFactory
    {
        public XmlTypeMapping Create(ITaskItem taskItem)
        {
            if (taskItem == null)
                throw new ArgumentNullException("taskItem");

            var xmlTypeCode = taskItem.ItemSpec;

            var codeTypeReference = taskItem.GetMetadata("CodeTypeReference");
            if (codeTypeReference == null)
                throw new Exception("CodeTypeReference cannot be null.");

            return new XmlTypeMapping
                {
                    XmlTypeCode = (XmlTypeCode) Enum.Parse(typeof (XmlTypeCode), xmlTypeCode, false),
                    CodeTypeReference = new CodeTypeReference(codeTypeReference)
                };
        }
    }
}
