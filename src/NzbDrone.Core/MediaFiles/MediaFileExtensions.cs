using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Qualities;


namespace NzbDrone.Core.MediaFiles
{
    public static class MediaFileExtensions
    {
        private static Dictionary<string, Source> _fileExtensions;
        private static Dictionary<string, Resolution> _resolutionExt;

        static MediaFileExtensions()
        {
            _fileExtensions = new Dictionary<string, Source>
            {
                //Unknown
                { ".webm", Source.UNKNOWN },

                //SDTV
                { ".m4v", Source.TV },
                { ".3gp", Source.TV },
                { ".nsv", Source.TV },
                { ".ty", Source.TV },
                { ".strm", Source.TV },
                { ".rm", Source.TV },
                { ".rmvb", Source.TV },
                { ".m3u", Source.TV },
                { ".ifo", Source.TV },
                { ".mov", Source.TV },
                { ".qt", Source.TV },
                { ".divx", Source.TV },
                { ".xvid", Source.TV },
                { ".bivx", Source.TV },
                { ".nrg", Source.TV },
                { ".pva", Source.TV },
                { ".wmv", Source.TV },
                { ".asf", Source.TV },
                { ".asx", Source.TV },
                { ".ogm", Source.TV },
                { ".ogv", Source.TV },
                { ".m2v", Source.TV },
                { ".avi", Source.TV },
                { ".bin", Source.TV },
                { ".dat", Source.TV },
                { ".dvr-ms", Source.TV },
                { ".mpg", Source.TV },
                { ".mpeg", Source.TV },
                { ".mp4", Source.TV },
                { ".avc", Source.TV },
                { ".vp3", Source.TV },
                { ".svq3", Source.TV },
                { ".nuv", Source.TV },
                { ".viv", Source.TV },
                { ".dv", Source.TV },
                { ".fli", Source.TV },
                { ".flv", Source.TV },
                { ".wpl", Source.TV },

                //DVD
                { ".img", Source.DVD },
                { ".iso", Source.DVD },
                { ".vob", Source.DVD },

                //HD
                { ".mkv", Source.WEBDL },
                { ".ts", Source.TV },
                { ".wtv", Source.TV },

                //Bluray
                { ".m2ts", Source.BLURAY }
            };

            _resolutionExt = new Dictionary<string, Resolution>
            {
                //HD
                { ".mkv", Resolution.R720P },
                { ".ts", Resolution.R720P },
                { ".wtv", Resolution.R720P },

                //Bluray
                { ".m2ts", Resolution.R720P }
            };
        }

        public static HashSet<string> Extensions => new HashSet<string>(_fileExtensions.Keys, StringComparer.OrdinalIgnoreCase);

        public static Source GetSourceForExtension(string extension)
        {
            if (_fileExtensions.ContainsKey(extension))
            {
                return _fileExtensions[extension];
            }

            return Source.UNKNOWN;
        }

        public static Resolution GetResolutionForExtension(string extension)
        {
            if (_resolutionExt.ContainsKey(extension))
            {
                return _resolutionExt[extension];
            }

            var source = Source.UNKNOWN;
            if (_fileExtensions.ContainsKey(extension))
            {
                source = _fileExtensions[extension];
            }

            if (source == Source.DVD || source == Source.TV)
            {
                return Resolution.R480P;
            }

            return Resolution.Unknown;
        }
    }
}
