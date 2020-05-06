using System;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace VersOne.Epub.Internal
{
    public static class RootFilePathReader
    {
        public static async Task<string> GetRootFilePathAsync(ZipArchive epubArchive)
        {
            const string EPUB_CONTAINER_FILE_PATH = "META-INF/container.xml";
            var containerFileEntry = epubArchive.GetEntry(EPUB_CONTAINER_FILE_PATH);
            if (containerFileEntry == null)
            {
                throw new Exception($"EPUB parsing error: {EPUB_CONTAINER_FILE_PATH} file not found in archive.");
            }

            XDocument containerDocument;
            using (var containerStream = containerFileEntry.Open())
            {
                containerDocument = await XmlUtils.LoadDocumentAsync(containerStream).ConfigureAwait(false);
            }

            XNamespace cnsNamespace = "urn:oasis:names:tc:opendocument:xmlns:container";
            var fullPathAttribute = containerDocument.Element(cnsNamespace + "container")?.Element(cnsNamespace + "rootfiles")?.Element(cnsNamespace + "rootfile")?.Attribute("full-path");
            if (fullPathAttribute == null)
            {
                throw new Exception("EPUB parsing error: root file path not found in the EPUB container.");
            }

            return fullPathAttribute.Value;
        }
    }
}
