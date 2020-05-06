using VersOne.Epub.Internal;

namespace VersOne.Epub.Schema
{
    public class EpubPackage
    {
        public EpubVersion EpubVersion { get; set; }
        public EpubMetadata Metadata { get; set; }

        public string GetVersionString()
        {
            return VersionUtils.GetVersionString(EpubVersion);
        }
    }
}
