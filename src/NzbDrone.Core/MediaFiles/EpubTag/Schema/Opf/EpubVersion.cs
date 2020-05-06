using System;

namespace VersOne.Epub.Schema
{
    public enum EpubVersion
    {
        [VersionString("2.0")]
        EPUB_2 = 2,

        [VersionString("3.0")]
        EPUB_3_0,

        [VersionString("3.1")]
        EPUB_3_1
    }

    public class VersionStringAttribute : Attribute
    {
        public VersionStringAttribute(string version)
        {
            Version = version;
        }

        public string Version { get; }
    }
}
