using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles.EpisodeImport
{
    public interface IDetectTrailer
    {
        bool IsTrailer(Series series, QualityModel quality, string path, long size, bool isSpecial);
        bool IsTrailer(Movie movie, QualityModel quality, string path, long size, bool isSpecial);
    }

    public class DetectTrailer : IDetectTrailer
    {
        private readonly IVideoFileInfoReader _videoFileInfoReader;
        private readonly Logger _logger;

        private static List<Quality> _largeTrailerSizeQualities = new List<Quality>
        {
            Quality.HDTV1080p,
            Quality.WEBDL1080p,
            Quality.Bluray1080p,
            Quality.Remux1080p,
            Quality.Remux2160p,
            Quality.Bluray2160p
        };

        public DetectTrailer(IVideoFileInfoReader videoFileInfoReader, Logger logger)
        {
            _videoFileInfoReader = videoFileInfoReader;
            _logger = logger;
        }

        public static long TrailerSizeLimit => 150.Megabytes();

        public bool IsTrailer(Series series, QualityModel quality, string path, long size, bool isSpecial)
        {
            return false;
        }

        public bool IsTrailer(Movie movie, QualityModel quality, string path, long size, bool isSpecial)
        {
            var filename = Path.GetFileNameWithoutExtension(path);

            try
            {
                var runTime = _videoFileInfoReader.GetRunTime(path);
                var minimumRuntime = GetMinimumAllowedRuntime(movie);

                if (runTime.TotalMinutes.Equals(0))
                {
                    _logger.Error("[{0}] has a runtime of 0, is it a valid video file?", path);
                    return true;
                }

                if (runTime.TotalSeconds < minimumRuntime && filename.ContainsIgnoreCase("trailer"))
                {
                    _logger.Debug("[{0}] appears to be a trailer. Runtime: {1} seconds. Expected at least: {2} seconds", path, runTime, minimumRuntime);
                    return true;
                }
            }

            catch (DllNotFoundException)
            {
                _logger.Debug("Falling back to file size detection");

                return CheckSize(filename, size, quality);
            }

            _logger.Debug("Runtime is over 90 seconds");
            return false;
        }

        private bool CheckSize(string filename, long size, QualityModel quality)
        {
            if (_largeTrailerSizeQualities.Contains(quality.Quality))
            {
                if (size < TrailerSizeLimit * 2 && filename.ContainsIgnoreCase("trailer"))
                {
                    _logger.Debug("1080p file is less than trailer limit");
                    return true;
                }
            }

            if (size < TrailerSizeLimit && filename.ContainsIgnoreCase("trailer"))
            {
                _logger.Debug("File is less than trailer limit");
                return true;
            }

            return false;
        }

        private int GetMinimumAllowedRuntime(Movie movie)
        {
            if (movie.Runtime < 1)
            {
                return 5 * 60;
            }

            return movie.Runtime / 5 * 60;
        }
    }
}
