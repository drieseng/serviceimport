using System.CodeDom;
using System.Collections.Generic;
using System.Xml.Schema;

namespace ServiceImport.Console
{
    public class Options
    {
        public Options(string wsdl, string outputDirectory, IDictionary<XmlTypeCode, CodeTypeReference> xmlTypeMappings, IDictionary<string, string> namespaceMappings)
        {
            Wsdl = wsdl;
            OutputDirectory = outputDirectory;
            XmlTypeMappings = xmlTypeMappings;
            NamespaceMappings = namespaceMappings;
        }

        public string Wsdl
        {
            get; private set;
        }

        public string OutputDirectory
        {
            get; private set;
        }

        public IDictionary<XmlTypeCode, CodeTypeReference> XmlTypeMappings
        {
            get; private set;
        }

        public IDictionary<string,string> NamespaceMappings
        {
            get; private set;
        }


    }
}
