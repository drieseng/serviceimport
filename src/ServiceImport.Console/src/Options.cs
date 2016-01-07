﻿using System.CodeDom;
using System.Collections.Generic;
using System.Xml.Schema;
using BRail.Nis.ServiceImport.Framework.CodeDom;

namespace ServiceImport.Console
{
    public class Options
    {
        public Options(string wsdl, string outputDirectory, IDictionary<XmlTypeCode, CodeTypeReference> xmlTypeMappings, IDictionary<string, string> namespaceMappings, IDictionary<string, TypeAccessModifier> typeAccessModifierMappings)
        {
            Wsdl = wsdl;
            OutputDirectory = outputDirectory;
            XmlTypeMappings = xmlTypeMappings;
            NamespaceMappings = namespaceMappings;
            TypeAccessModifierMappings = typeAccessModifierMappings;
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

        public IDictionary<string, TypeAccessModifier> TypeAccessModifierMappings
        {
            get; private set;
        }
    }
}
