using System.IO.Compression;
using System.Threading.Tasks;
using VersOne.Epub.Schema;

namespace VersOne.Epub.Internal
{
    public static class SchemaReader
    {
        public static async Task<EpubSchema> ReadSchemaAsync(ZipArchive epubArchive)
        {
            var result = new EpubSchema();
            var rootFilePath = await RootFilePathReader.GetRootFilePathAsync(epubArchive).ConfigureAwait(false);
            var contentDirectoryPath = ZipPathUtils.GetDirectoryPath(rootFilePath);
            result.ContentDirectoryPath = contentDirectoryPath;
            EpubPackage package = await PackageReader.ReadPackageAsync(epubArchive, rootFilePath).ConfigureAwait(false);
            result.Package = package;
            return result;
        }
    }
}
