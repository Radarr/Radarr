using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Extras.Subtitles
{
    public static class SubtitleFileExtensions
    {
        public static HashSet<string> Extensions => new (StringComparer.OrdinalIgnoreCase)
        {
            ".aqt",
            ".ass",
            ".idx",
            ".jss",
            ".psb",
            ".rt",
            ".smi",
            ".srt",
            ".ssa",
            ".sub",
            ".sup",
            ".txt",
            ".utf",
            ".utf8",
            ".utf-8",
            ".vtt"
        };
    }
}
