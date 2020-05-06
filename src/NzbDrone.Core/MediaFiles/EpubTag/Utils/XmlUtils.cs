using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace VersOne.Epub.Internal
{
    public static class XmlUtils
    {
        public static async Task<XDocument> LoadDocumentAsync(Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream).ConfigureAwait(false);
                memoryStream.Position = 0;
                var xmlReaderSettings = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Ignore,
                    Async = true
                };
                using (var xmlReader = XmlReader.Create(memoryStream, xmlReaderSettings))
                {
                    return await Task.Run(() => LoadXDocument(memoryStream)).ConfigureAwait(false);
                }
            }
        }

        private static XDocument LoadXDocument(MemoryStream memoryStream)
        {
            try
            {
                return XDocument.Load(memoryStream);
            }
            catch (XmlException)
            {
                // .NET can't handle XML 1.1, so try sanitising and reading as 1.0
                memoryStream.Position = 0;
                using (var sr = new StreamReader(memoryStream))
                {
                    var text = sr.ReadToEnd();

                    if (text.StartsWith(@"<?xml version=""1.1"""))
                    {
                        text = @"<?xml version=""1.0""" + text.Substring(19);

                        var chars = text.Where(x => XmlConvert.IsXmlChar(x)).ToArray();
                        var sanitised = new string(chars);

                        return XDocument.Parse(sanitised);
                    }
                }

                throw;
            }
        }
    }
}
