using System.CodeDom;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Xml.Schema;
using ServiceImport.Framework.CodeDom;

namespace ServiceImport.Framework.Extension
{
    /// <summary>
    /// Remove the <see cref="ServiceContractAttribute.ConfigurationName"/> argument from any
    /// <see cref="ServiceContractAttribute"/> that is applied to a type.
    /// </summary>
    /// <remarks>
    /// <para>
    /// By default, the WSDL importer generates a service interface in the default namespace, and
    /// <see cref="ServiceContractAttribute.ConfigurationName"/> of the <see cref="ServiceContractAttribute"/>
    /// that is applied to the service interface reflects this.
    /// </para>
    /// <para>
    /// When the type name of the service interface is renamed (through the <see cref="TypeRenameExtension"/>), the
    /// value of <c></c>configuration name no longer matches the qualified type name of that interface.
    /// </para>
    /// <para>
    /// This extension removes the <see cref="ServiceContractAttribute.ConfigurationName"/> argument from the
    /// <see cref="ServiceContractAttribute"/>, and hereby ensures that WCF will always use the qualified name of
    /// the service interface as <see cref="ServiceContractAttribute.ConfigurationName"/>.
    /// </para>
    /// </remarks>
    public class RemoveServiceContractConfigurationNameExtension : IXsdImportExtension
    {
        public void BeforeImport(XmlSchemaSet xmlSchemas)
        {
        }

        public void ImportContract(XsdDataContractImporter importer)
        {
        }

        public void GenerateContract(CodeCompileUnit compileUnit)
        {
            foreach (CodeNamespace ns in compileUnit.Namespaces)
            {
                foreach (CodeTypeDeclaration type in ns.Types)
                {
                    foreach (CodeAttributeDeclaration customAttribute in type.CustomAttributes)
                    {
                        if (customAttribute.AttributeType.BaseType == typeof(ServiceContractAttribute).FullName)
                        {
                            var configurationName = customAttribute.Arguments.FindArgumentByName(nameof(ServiceContractAttribute.ConfigurationName));
                            if (configurationName != null)
                            {
                                customAttribute.Arguments.Remove(configurationName);
                            }
                        }
                    }
                }
            }
        }
    }
}
