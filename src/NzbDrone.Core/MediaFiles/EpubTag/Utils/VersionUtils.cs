using System.Reflection;
using VersOne.Epub.Schema;

namespace VersOne.Epub.Internal
{
    public static class VersionUtils
    {
        public static string GetVersionString(EpubVersion epubVersion)
        {
            var epubVersionType = typeof(EpubVersion);
            var fieldInfo = epubVersionType.GetRuntimeField(epubVersion.ToString());

            if (fieldInfo != null)
            {
                return fieldInfo.GetCustomAttribute<VersionStringAttribute>().Version;
            }
            else
            {
                return epubVersion.ToString();
            }
        }
    }
}
