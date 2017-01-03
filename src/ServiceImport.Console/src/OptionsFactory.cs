using System.CodeDom;
using System.Collections.Generic;
using System.Xml.Schema;
using BRail.Nis.ServiceImport.Framework.CodeDom;

namespace ServiceImport.Console
{
    public class OptionsFactory
    {
        public Options Create(string[] args)
        {
            var wsdl = args[0];
            var outputDirectory = args[1];

            var xmlTypeMappings = new Dictionary<XmlTypeCode, CodeTypeReference>
                {
                    { XmlTypeCode.Date, new CodeTypeReference("BRail.Nis.GeneralLib.ContractEntities.Date") }
                };

            var namespaceMappings = new Dictionary<string, string>
                {
                    { "http://www.infrabel.be/A204/Nis", "BRail.Nis.WcfContract" },
                    { "http://www.infrabel.be/A204/Nis/Obstruction", "BRail.Nis.GeneralLib.ContractEntities.Obstruction" }
                };

            var typeAccessModifierMappings = new Dictionary<string, TypeAccessModifier>
                {
                    { "BRail.Nis.WcfContract.ObstructionServiceClient", TypeAccessModifier.Internal }
                };

            var typeRenameMappings = new Dictionary<string, string>
                {
                    {
                        "BRail.Nis.WcfContract.ObstructionServiceClient",
                        "BRail.Nis.ClientLib.ObstructionServiceReference.ObstructionServiceClient"
                    }
                };

            return new Options(wsdl, outputDirectory, xmlTypeMappings, namespaceMappings, typeAccessModifierMappings, typeRenameMappings);
        }
    }
}
