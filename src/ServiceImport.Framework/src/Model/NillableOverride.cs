using System.Xml;

namespace ServiceImport.Framework.Model
{
    public class NillableOverride
    {
        public XmlQualifiedName ComplexTypeName { get; set; }
        public string ElementName { get; set; }
        public bool IsNillable { get; set; }
    }
}
