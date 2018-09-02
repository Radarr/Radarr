using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.MediaFiles.MovieImport
{
    public interface IDetectSample
    {
        bool IsSample(Movie movie, QualityModel quality, string path, long size, bool isSpecial);
    }

    public class DetectSample : IDetectSample
    {
        private readonly IVideoFileInfoReader _videoFileInfoReader;
        private readonly Logger _logger;

        //private static List<Quality> _largeSampleSizeQualities = new List<Quality> { Quality.HDTV1080p, Quality.WEBDL1080p, Quality.Bluray1080p };
        private static List<Resolution> _largeSampleSizeResolutions = new List<Resolution>{Resolution.R1080P, Resolution.R2160P};

        public DetectSample(IVideoFileInfoReader videoFileInfoReader, Logger logger)
        {
            _videoFileInfoReader = videoFileInfoReader;
            _logger = logger;
        }

        public static long SampleSizeLimit => 70.Megabytes();

        public bool IsSample(Movie movie, QualityModel quality, string path, long size, bool isSpecial)
        {
            if (isSpecial)
            {
                _logger.Debug("Special, skipping sample check");
                return false;
            }

            var extension = Path.GetExtension(path);

            if (extension != null && extension.Equals(".flv", StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.Debug("Skipping sample check for .flv file");
                return false;
            }

            if (extension != null && extension.Equals(".strm", StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.Debug("Skipping sample check for .strm file");
                return false;
            }

            try
            {
                var runTime = _videoFileInfoReader.GetRunTime(path);
                var minimumRuntime = GetMinimumAllowedRuntime(movie);

                if (runTime.TotalMinutes.Equals(0))
                {
                    _logger.Error("[{0}] has a runtime of 0, is it a valid video file?", path);
                    return true;
                }

                if (runTime.TotalSeconds < minimumRuntime)
                {
                    _logger.Debug("[{0}] appears to be a sample. Runtime: {1} seconds. Expected at least: {2} seconds", path, runTime, minimumRuntime);
                    return true;
                }
            }

            catch (DllNotFoundException)
            {
                _logger.Debug("Falling back to file size detection");

                return CheckSize(size, quality);
            }

            _logger.Debug("Runtime is over 90 seconds");
            return false;
        }

        private bool CheckSize(long size, QualityModel quality)
        {
            if (_largeSampleSizeResolutions.Contains(quality.Resolution))
            {
                if (size < SampleSizeLimit * 2)
                {
                    _logger.Debug("1080p file is less than sample limit");
                    return true;
                }
            }

            if (size < SampleSizeLimit)
            {
                _logger.Debug("File is less than sample limit");
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
