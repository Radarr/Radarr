using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using VersOne.Epub.Internal;

namespace VersOne.Epub
{
    public static class EpubReader
    {
        /// <summary>
        /// Opens the book synchronously without reading its whole content. Holds the handle to the EPUB file.
        /// </summary>
        /// <param name="filePath">path to the EPUB file</param>
        /// <returns></returns>
        public static EpubBookRef OpenBook(string filePath)
        {
            return OpenBookAsync(filePath).Result;
        }

        /// <summary>
        /// Opens the book asynchronously without reading its whole content. Holds the handle to the EPUB file.
        /// </summary>
        /// <param name="filePath">path to the EPUB file</param>
        /// <returns></returns>
        public static Task<EpubBookRef> OpenBookAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                if (!filePath.StartsWith(@"\\?\"))
                {
                    filePath = @"\\?\" + filePath;
                }

                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException("Specified epub file not found.", filePath);
                }
            }

            return OpenBookAsync(GetZipArchive(filePath));
        }

        private static async Task<EpubBookRef> OpenBookAsync(ZipArchive zipArchive, string filePath = null)
        {
            EpubBookRef result = null;
            try
            {
                result = new EpubBookRef(zipArchive);
                result.FilePath = filePath;
                result.Schema = await SchemaReader.ReadSchemaAsync(zipArchive).ConfigureAwait(false);
                result.Title = result.Schema.Package.Metadata.Titles.FirstOrDefault() ?? string.Empty;
                result.AuthorList = result.Schema.Package.Metadata.Creators.Select(creator => creator.Creator).ToList();
                result.Author = string.Join(", ", result.AuthorList);
                return result;
            }
            catch
            {
                result?.Dispose();
                throw;
            }
        }

        private static ZipArchive GetZipArchive(string filePath)
        {
            return ZipFile.OpenRead(filePath);
        }
    }
}
