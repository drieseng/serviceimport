using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;

namespace ServiceImport.Framework.Model
{
    public class ServiceModel
    {
        public ServiceModel()
        {
            ComplexTypes = new List<ComplexType>();
            OperationMessages = new Dictionary<XmlQualifiedName, OperationMessageInfo>();
        }

        public IList<ComplexType> ComplexTypes
        {
            get; private set;
        }

        public XmlSchemaSet XmlSchemas
        {
            get; set;
        }

        public IDictionary<XmlQualifiedName, OperationMessageInfo> OperationMessages
        {
            get; private set;
        }
    }
}
