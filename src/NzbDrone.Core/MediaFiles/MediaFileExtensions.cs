using System;
using System.Collections.Generic;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles
{
    public static class MediaFileExtensions
    {
        private static Dictionary<string, Quality> _fileExtensions;

        static MediaFileExtensions()
        {
            _fileExtensions = new Dictionary<string, Quality>(StringComparer.OrdinalIgnoreCase)
            {
                // Unknown
                { ".webm", Quality.Unknown },

                // SDTV
                { ".m4v", Quality.SDTV },
                { ".3gp", Quality.SDTV },
                { ".nsv", Quality.SDTV },
                { ".ty", Quality.SDTV },
                { ".strm", Quality.SDTV },
                { ".rm", Quality.SDTV },
                { ".rmvb", Quality.SDTV },
                { ".m3u", Quality.SDTV },
                { ".ifo", Quality.SDTV },
                { ".mov", Quality.SDTV },
                { ".qt", Quality.SDTV },
                { ".divx", Quality.SDTV },
                { ".xvid", Quality.SDTV },
                { ".bivx", Quality.SDTV },
                { ".nrg", Quality.SDTV },
                { ".pva", Quality.SDTV },
                { ".wmv", Quality.SDTV },
                { ".asf", Quality.SDTV },
                { ".asx", Quality.SDTV },
                { ".ogm", Quality.SDTV },
                { ".ogv", Quality.SDTV },
                { ".m2v", Quality.SDTV },
                { ".avi", Quality.SDTV },
                { ".bin", Quality.SDTV },
                { ".dat", Quality.SDTV },
                { ".dvr-ms", Quality.SDTV },
                { ".mpg", Quality.SDTV },
                { ".mpeg", Quality.SDTV },
                { ".mp4", Quality.SDTV },
                { ".avc", Quality.SDTV },
                { ".vp3", Quality.SDTV },
                { ".svq3", Quality.SDTV },
                { ".nuv", Quality.SDTV },
                { ".viv", Quality.SDTV },
                { ".dv", Quality.SDTV },
                { ".fli", Quality.SDTV },
                { ".flv", Quality.SDTV },
                { ".wpl", Quality.SDTV },

                // DVD
                { ".img", Quality.DVD },
                { ".iso", Quality.DVD },
                { ".vob", Quality.DVD },

                // HD
                { ".mkv", Quality.WEBDL720p },
                { ".mk3d", Quality.WEBDL720p },
                { ".ts", Quality.SDTV },
                { ".wtv", Quality.SDTV },

                // Bluray
                { ".m2ts", Quality.Bluray720p },

                // eBooks
                { ".epub", Quality.EPUB },
                { ".mobi", Quality.MOBI },
                { ".azw", Quality.AZW3 },
                { ".azw3", Quality.AZW3 },
                { ".pdf", Quality.PDF },
                { ".txt", Quality.TXT },
                { ".djvu", Quality.PDF },
                { ".cbr", Quality.EbookUnknown },
                { ".cbz", Quality.EbookUnknown },

                // Audiobooks
                { ".m4b", Quality.M4B },
                { ".aa", Quality.MP3_128 },
                { ".aax", Quality.M4B },

                // Music - Lossy
                { ".mp3", Quality.MusicMP3_320 },
                { ".aac", Quality.MusicAAC_256 },
                { ".ogg", Quality.MusicOGG_320 },
                { ".oga", Quality.MusicOGG_320 },
                { ".opus", Quality.MusicOpus_192 },
                { ".wma", Quality.MusicWMA },
                { ".m4a", Quality.MusicAAC_256 },

                // Music - Lossless (default to CD quality, actual detected via MusicFileAnalyzer)
                { ".flac", Quality.MusicFLAC_16_44 },
                { ".wav", Quality.MusicWAV_16_44 },
                { ".aiff", Quality.MusicAIFF_16_44 },
                { ".aif", Quality.MusicAIFF_16_44 },
                { ".alac", Quality.MusicALAC_16_44 },
                { ".ape", Quality.MusicAPE },
                { ".wv", Quality.MusicWavPack },
                { ".dsf", Quality.MusicDSD64 },
                { ".dff", Quality.MusicDSD64 }
            };
        }

        public static HashSet<string> Extensions => new HashSet<string>(_fileExtensions.Keys, StringComparer.OrdinalIgnoreCase);
        public static HashSet<string> DiskExtensions => new HashSet<string>(new[] { ".img", ".iso", ".vob" }, StringComparer.OrdinalIgnoreCase);
        public static HashSet<string> EbookExtensions => new HashSet<string>(new[] { ".epub", ".mobi", ".azw", ".azw3", ".pdf", ".txt", ".djvu", ".cbr", ".cbz" }, StringComparer.OrdinalIgnoreCase);
        public static HashSet<string> AudiobookExtensions => new HashSet<string>(new[] { ".m4b", ".aa", ".aax" }, StringComparer.OrdinalIgnoreCase);
        public static HashSet<string> MusicExtensions => new HashSet<string>(new[] { ".mp3", ".aac", ".ogg", ".oga", ".opus", ".wma", ".m4a", ".flac", ".wav", ".aiff", ".aif", ".alac", ".ape", ".wv", ".dsf", ".dff" }, StringComparer.OrdinalIgnoreCase);

        public static Quality GetQualityForExtension(string extension)
        {
            if (_fileExtensions.TryGetValue(extension, out var quality))
            {
                return quality;
            }

            return Quality.Unknown;
        }
    }
}
