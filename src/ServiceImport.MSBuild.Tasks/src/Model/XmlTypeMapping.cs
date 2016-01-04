using System.CodeDom;
using System.Xml.Schema;

namespace BRail.Nis.ServiceImport.MSBuild.Tasks.Model
{
    internal class XmlTypeMapping
    {
        public XmlTypeCode XmlTypeCode { get; set; }

        public CodeTypeReference CodeTypeReference { get; set; }
    }
}
