using System.Xml;

namespace ServiceImport.MSBuild.Factory
{
    public class XmlQualifiedNameFactory
    {
        public XmlQualifiedName Create(string qualifiedName)
        {
            var lastColonIndex = qualifiedName.LastIndexOf(':');

            if (lastColonIndex != -1)
            {
                var localName = qualifiedName.Substring(lastColonIndex + 1);
                var ns = qualifiedName.Substring(0, lastColonIndex);
                return new XmlQualifiedName(localName, ns);
            }

            return new XmlQualifiedName(qualifiedName);
        }
    }
}
