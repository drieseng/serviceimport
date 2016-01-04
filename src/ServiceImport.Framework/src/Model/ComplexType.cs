using System.Collections.Generic;
using System.Xml;

namespace BRail.Nis.ServiceImport.Framework.Model
{
    public class ComplexType
    {
        public ComplexType(XmlQualifiedName qualifiedName, bool isAbstract, IDictionary<string, Element> elements)
        {
            QualifiedName = qualifiedName;
            IsAbstract = isAbstract;
            Elements = elements;
        }

        public bool IsAbstract { get; private set; }

        public IDictionary<string, Element> Elements { get; private set; }

        public XmlQualifiedName QualifiedName { get; private set; }
    }
}
