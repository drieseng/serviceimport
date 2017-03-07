using System.Collections.Generic;
using System.Xml;

namespace ServiceImport.Framework.Model
{
    public class ComplexType
    {
        internal ComplexType(XmlQualifiedName qualifiedName, bool isAbstract, IList<Element> elements)
        {
            QualifiedName = qualifiedName;
            IsAbstract = isAbstract;
            Elements = elements;
        }

        public bool IsAbstract { get; private set; }

        public IList<Element> Elements { get; private set; }

        public XmlQualifiedName QualifiedName { get; private set; }
    }
}
