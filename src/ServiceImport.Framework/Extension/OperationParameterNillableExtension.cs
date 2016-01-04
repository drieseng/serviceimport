using BRail.Nis.ServiceImport.Framework.Helper;
using BRail.Nis.ServiceImport.Framework.Model;
using System;
using System.Collections.Generic;
using System.ServiceModel.Description;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Schema;
using SWSServiceDecription = System.Web.Services.Description.ServiceDescription;

namespace BRail.Nis.ServiceImport.Framework.Extension
{
    /// <summary>
    /// Modifies <see cref="XmlSchemaElement.IsNillable"/> to <c>true</c> for elements which WCF considers to be backed
    /// by a reference type or where <see cref="XmlSchemaParticle.MinOccurs"/> is <c>zero</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If an operation parameter backed by a reference type is not marked <see cref="XmlSchemaElement.IsNillable"/>, then
    /// WCF will not unwrap the operation message.
    /// </para>
    /// <para>
    /// When an operation parameter backed by a value type is optional, meaning <see cref="XmlSchemaParticle.MinOccurs"/> is <c>zero</c>,
    /// and is not marked <see cref="XmlSchemaElement.IsNillable"/>, then the corresponding argument in the .NET method will not be nullable.
    /// </para>
    /// </remarks>
    public class OperationParameterNillableExtension : IWsdlImportExtension
    {
        private readonly ServiceModel _serviceModel;

        public OperationParameterNillableExtension(ServiceModel serviceModel)
        {
            _serviceModel = serviceModel;
        }

        #region IWsdlImportExtension implementation

        public void BeforeImport(ServiceDescriptionCollection wsdlDocuments, XmlSchemaSet xmlSchemas, ICollection<XmlElement> policy)
        {
            foreach (SWSServiceDecription wsdl in wsdlDocuments)
            {
                foreach (PortType portType in wsdl.PortTypes)
                {
                    foreach (Operation operation in portType.Operations)
                    {
                        foreach (OperationMessage operationMessage in operation.Messages)
                        {
                            foreach (Message message in wsdl.Messages)
                            {
                                if (message.Name != operationMessage.Message.Name)
                                    continue;

                                var parametersPart = message.Parts.SingleOrDefault<MessagePart>(p => p.Name == "parameters");
                                if (parametersPart == null)
                                    continue;

                                var wrapperElement = _serviceModel.XmlSchemas.GlobalElements[parametersPart.Element] as XmlSchemaElement;
                                if (wrapperElement == null)
                                    throw new Exception(string.Format("Wrapper element '{0}' could not be found.", parametersPart.Element));

                                foreach (var parameterElement in wrapperElement.GetWrapperElementParameters())
                                {
                                    if (parameterElement.IsReferenceType() || parameterElement.MinOccurs == 0)
                                        parameterElement.IsNillable = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void ImportContract(WsdlImporter importer, WsdlContractConversionContext context)
        {
        }

        public void ImportEndpoint(WsdlImporter importer, WsdlEndpointConversionContext context)
        {
        }

        #endregion IWsdlImportExtension implementation
    }
}
