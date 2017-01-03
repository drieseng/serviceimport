using System.Xml.Schema;

namespace BRail.Nis.ServiceImport.Framework.Model
{
    public class Element
    {
        private readonly XmlSchemaElement _schemaElement;

        internal Element(XmlSchemaElement schemaElement)
        {
            _schemaElement = schemaElement;
        }

        public string Name => _schemaElement.Name;

        public XmlTypeCode TypeCode => _schemaElement.ElementSchemaType.TypeCode;

        public decimal MaxOccurs => _schemaElement.MaxOccurs;

        public decimal MinOccurs => _schemaElement.MinOccurs;

        public bool IsNillable {
            get { return _schemaElement.IsNillable; }
            set { _schemaElement.IsNillable = value; }
        }
    }
}
