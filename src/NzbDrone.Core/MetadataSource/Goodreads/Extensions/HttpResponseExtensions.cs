using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using NzbDrone.Common.Http;
using NzbDrone.Core.MetadataSource.SkyHook;

namespace NzbDrone.Core.MetadataSource.Goodreads
{
    public static class HttpResponseExtensions
    {
        public static T Deserialize<T>(this HttpResponse response, string elementName = null)
            where T : GoodreadsResource, new()
        {
            response.ThrowIfException();

            try
            {
                var document = XDocument.Parse(response.Content);
                if (document.Root == null ||
                    document.Root.Name == "error")
                {
                    return null;
                }
                else
                {
                    var root = document.Element("GoodreadsResponse") ?? (XNode)document;
                    var responseObject = new T();
                    var contentRoot = root.XPathSelectElement(elementName ?? responseObject.ElementName);

                    responseObject.Parse(contentRoot);
                    return responseObject;
                }
            }
            catch (XmlException)
            {
                return null;
            }
        }

        private static void ThrowIfException(this HttpResponse response)
        {
            // Try and find an error from the Goodreads response
            string error = null;
            try
            {
                var document = XDocument.Parse(response.Content);

                // Goodreads returns several different types of errors...
                if (document.Root != null)
                {
                    if (document.Root.Name == "error")
                    {
                        // One is a single XML error node
                        var element = document.Element("error");
                        if (element != null)
                        {
                            error = element.Value;
                        }
                    }
                    else if (document.Root.Name == "errors")
                    {
                        // Another one is a list of XML error nodes
                        var element = document.Element("errors");
                        var children = element?.Descendants("error");
                        if (children.Any())
                        {
                            error = string.Join(Environment.NewLine, children.Select(x => x.Value));
                        }
                    }
                    else if (document.Root.Name == "hash")
                    {
                        // And another one is in a "hash" XML object
                        var element = document.Element("hash");
                        if (element != null)
                        {
                            var status = element.ElementAsString("status");
                            var message = element.ElementAsString("error");
                            if (!string.IsNullOrEmpty(message))
                            {
                                error = string.Join(" ", status, message);
                            }
                        }
                    }
                    else
                    {
                        // Yet another one is an entire XML structure with multiple messages...
                        var element = document.XPathSelectElement("GoodreadsResponse/error");
                        if (element != null)
                        {
                            // There are four total error messages
                            var plain = element.Value;
                            var genericMessage = element.ElementAsString("generic");
                            var detailMessage = element.ElementAsString("detail");
                            var friendlyMessage = element.ElementAsString("friendly");

                            // Use the best message that exists...
                            error = friendlyMessage ?? detailMessage ?? genericMessage ?? plain;
                        }
                    }
                }
            }
            catch (XmlException)
            {
                // We don't really care if any exception was thrown above
                // we're just trying to find an error message after all...
            }

            // If we found any error at all above, throw an exception
            if (!string.IsNullOrWhiteSpace(error))
            {
                throw new SkyHookException("Received an error from Goodreads " + error);
            }
        }
    }
}
