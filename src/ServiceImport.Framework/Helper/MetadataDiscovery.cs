using System;
using System.Collections.ObjectModel;
using System.ServiceModel.Description;
using System.Web.Services.Discovery;
using System.Xml.Schema;
using SWSServiceDescription = System.Web.Services.Description.ServiceDescription;

namespace BRail.Nis.ServiceImport.Framework.Helper
{
    public class MetadataDiscovery
    {
        public MetadataSet Discover(string url)
        {
            DiscoveryClientProtocol disco = new DiscoveryClientProtocol();
            disco.AllowAutoRedirect = true;
            disco.UseDefaultCredentials = true;
            disco.DiscoverAny(url);
            disco.ResolveAll();
            var results = new Collection<MetadataSection>();
            foreach (object document in disco.Documents.Values)
                AddDocumentToResults(document, results);
            return new MetadataSet(results);
        }

        private static void AddDocumentToResults(object document, Collection<MetadataSection> results)
        {
            var serviceDescr = document as SWSServiceDescription;
            if (serviceDescr != null)
            {
                results.Add(MetadataSection.CreateFromServiceDescription(serviceDescr));
                return;
            }

            var schema = document as XmlSchema;
            if (schema != null)
            {
                results.Add(MetadataSection.CreateFromSchema(schema));
                return;
            }

            throw new NotSupportedException(string.Format("Metadata type '{0}' is currently not supported.", document.GetType().FullName));
        }
    }
}
