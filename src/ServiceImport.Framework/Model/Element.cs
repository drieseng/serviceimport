using System.Xml.Schema;

namespace BRail.Nis.ServiceImport.Framework.Model
{
    public class Element
    {
        public Element(string name, XmlSchemaType schemaType, decimal minOccurs, decimal maxOccurs)
        {
            Name = name;
            SchemaType = schemaType;
            MinOccurs = minOccurs;
            MaxOccurs = maxOccurs;
        }

        public string Name { get; private set; }

        public XmlSchemaType SchemaType { get; private set; }

        public decimal MaxOccurs { get; private set; }

        public decimal MinOccurs { get; private set; }
    }
}
