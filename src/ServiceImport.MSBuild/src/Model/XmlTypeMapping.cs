using System.CodeDom;
using System.Xml.Schema;

namespace ServiceImport.MSBuild.Model
{
    internal class XmlTypeMapping
    {
        public XmlTypeCode XmlTypeCode { get; set; }

        public CodeTypeReference CodeTypeReference { get; set; }
    }
}
