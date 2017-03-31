using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Hqub.MusicBrainz.API
{
    internal static class WebRequestHelper
    {
        private const string WebServiceUrl = "http://musicbrainz.org/ws/2/";
        private const string LookupTemplate = "{0}/{1}/?inc={2}";
        private const string BrowseTemplate = "{0}?{1}={2}&limit={3}&offset={4}&inc={5}";
        private const string SearchTemplate = "{0}?query={1}&limit={2}&offset={3}";

        internal async static Task<T> GetAsync<T>(string url, bool withoutMetadata = true) where T : Entities.Entity
        {
            try
            {
                var client = CreateHttpClient(true, Configuration.Proxy);

                return DeserializeStream<T>(await client.GetStreamAsync(url), withoutMetadata);
            }
            catch (Exception e)
            {
                if (Configuration.GenerateCommunicationThrow)
                {
                    throw e;
                }
            }

            return default(T);
        }

        /// <summary>
        /// Creates a webservice lookup template.
        /// </summary>
        internal static string CreateLookupUrl(string entity, string mbid, string inc)
        {
            return string.Format("{0}{1}", WebServiceUrl, string.Format(LookupTemplate, entity, mbid, inc));
        }

        /// <summary>
        /// Creates a webservice browse template.
        /// </summary>
        internal static string CreateBrowseTemplate(string entity, string relatedEntity, string mbid, int limit, int offset, string inc)
        {
            return string.Format("{0}{1}", WebServiceUrl, string.Format(BrowseTemplate, entity, relatedEntity, mbid, limit, offset, inc));
        }

        /// <summary>
        /// Creates a webservice search template.
        /// </summary>
        internal static string CreateSearchTemplate(string entity, string query, int limit, int offset)
        {
            query = Uri.EscapeUriString(query);

            return string.Format("{0}{1}", WebServiceUrl, string.Format(SearchTemplate, entity, query, limit, offset));
        }

        internal static T DeserializeStream<T>(Stream stream, bool withoutMetadata) where T : Entities.Entity
        {
            if (stream == null)
            {
                throw new NullReferenceException(Resources.Messages.EmptyStream);
            }

            var xml = XDocument.Load(stream);
            var serialize = new XmlSerializer(typeof(T));

            //Add extension namespace:
            var ns = new XmlSerializerNamespaces();
            ns.Add("ext", "http://musicbrainz.org/ns/ext#-2.0");

            //check valid xml schema:
            if (xml.Root == null || xml.Root.Name.LocalName != "metadata")
            {
                throw new NullReferenceException(Resources.Messages.WrongResponseFormat);
            }

            var node = withoutMetadata ? xml.Root.Elements().FirstOrDefault() : xml.Root;

            if (node == null)
            {
                return default(T);
            }

            return (T)serialize.Deserialize(node.CreateReader());
        }

        private static HttpClient CreateHttpClient(bool automaticDecompression = true, IWebProxy proxy = null)
        {
            var handler = new HttpClientHandler();

            if (proxy != null)
            {
                handler.Proxy = proxy;
                handler.UseProxy = true;
            }

            if (automaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }

            var client = new HttpClient(handler);

            client.DefaultRequestHeaders.Add("User-Agent", Configuration.UserAgent);

            return client;
        }
    }
}
