using System.IO;

namespace NzbDrone.Common.Extensions
{
    public static class PathExtensions
    {
        public static bool ContainsInvalidPathChars(this string text)
        {
            return text.IndexOfAny(Path.GetInvalidPathChars()) >= 0;
        }
    }
}
