using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Extras.Lyrics
{
    public static class LyricFileExtensions
    {
        private static HashSet<string> _fileExtensions;

        static LyricFileExtensions()
        {
            _fileExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                              {
                                  ".lrc",
                                  ".txt",
                                  ".utf",
                                  ".utf8",
                                  ".utf-8"
                              };
        }

        public static HashSet<string> Extensions => _fileExtensions;
    }
}
