using System.CodeDom;
using System.Xml.Schema;

namespace ServiceImport.Framework.Model
{
    public class XmlTypeMapping
    {
        public XmlTypeCode XmlTypeCode { get; set; }

        public CodeTypeReference CodeTypeReference { get; set; }

        public bool IsStruct { get; set; }

        public bool IsEnum { get; set; }
    }
}
