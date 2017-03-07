using System.Collections.Generic;
using System.Xml;

namespace ServiceImport.Framework.Model
{
    public class OperationMessageInfo
    {
        public OperationMessageInfo(XmlQualifiedName qualifiedName, IDictionary<string, OperationParameterInfo> parameters)
        {
            QualifiedName = qualifiedName;
            Parameters = parameters;
        }

        public XmlQualifiedName QualifiedName { get; private set; }

        public IDictionary<string, OperationParameterInfo> Parameters { get; private set; }
    }
}
