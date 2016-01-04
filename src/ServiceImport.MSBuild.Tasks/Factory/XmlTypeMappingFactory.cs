using BRail.Nis.ServiceImport.MSBuild.Tasks.Model;
using Microsoft.Build.Framework;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace BRail.Nis.ServiceImport.MSBuild.Tasks.Factories
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
                XmlTypeCode = (XmlTypeCode) Enum.Parse(typeof(XmlTypeCode), xmlTypeCode, false),
                CodeTypeReference = new CodeTypeReference(codeTypeReference)
            };
        }
    }
}
