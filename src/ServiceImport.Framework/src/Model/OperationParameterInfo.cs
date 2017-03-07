using System.Xml.Schema;

namespace ServiceImport.Framework.Model
{
    public class OperationParameterInfo
    {
        public OperationParameterInfo(XmlSchemaElement element)
        {
            Name = element.Name;
            MinOccurs = element.MinOccurs;
        }

        public string Name { get; private set; }

        public decimal MinOccurs { get; private set; }

    }
}
