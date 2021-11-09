using System;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.MediaFiles.MovieImport
{
    public interface IDetectSample
    {
        DetectSampleResult IsSample(Movie movie, string path);
    }

    public class DetectSample : IDetectSample
    {
        private readonly IVideoFileInfoReader _videoFileInfoReader;
        private readonly Logger _logger;

        public DetectSample(IVideoFileInfoReader videoFileInfoReader, Logger logger)
        {
            _videoFileInfoReader = videoFileInfoReader;
            _logger = logger;
        }

        public DetectSampleResult IsSample(Movie movie, string path)
        {
            var extension = Path.GetExtension(path);

            if (extension != null)
            {
                if (extension.Equals(".flv", StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.Debug("Skipping sample check for .flv file");
                    return DetectSampleResult.NotSample;
                }

                if (extension.Equals(".strm", StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.Debug("Skipping sample check for .strm file");
                    return DetectSampleResult.NotSample;
                }

                if (new string[] { ".iso", ".img", ".m2ts" }.Contains(extension, StringComparer.OrdinalIgnoreCase))
                {
                    _logger.Debug($"Skipping sample check for DVD/BR image file '{path}'");
                    return DetectSampleResult.NotSample;
                }
            }

            // TODO: Use MediaInfo from the import process, no need to re-process the file again here
            var runTime = _videoFileInfoReader.GetRunTime(path);

            if (!runTime.HasValue)
            {
                _logger.Error("Failed to get runtime from the file, make sure ffprobe is available");
                return DetectSampleResult.Indeterminate;
            }

            var minimumRuntime = GetMinimumAllowedRuntime(movie);

            if (runTime.Value.TotalMinutes.Equals(0))
            {
                _logger.Error("[{0}] has a runtime of 0, is it a valid video file?", path);
                return DetectSampleResult.Sample;
            }

            if (runTime.Value.TotalSeconds < minimumRuntime)
            {
                _logger.Debug("[{0}] appears to be a sample. Runtime: {1} seconds. Expected at least: {2} seconds", path, runTime.Value.TotalSeconds, minimumRuntime);
                return DetectSampleResult.Sample;
            }

            _logger.Debug("Runtime of {0} is more than {1} seconds, Not Sample", runTime.Value.TotalSeconds, minimumRuntime);
            return DetectSampleResult.NotSample;
        }

        private int GetMinimumAllowedRuntime(Movie movie)
        {
            //Anime short - 15 seconds
            if (movie.Runtime <= 3)
            {
                return 15;
            }

            //Webisodes - 90 seconds
            if (movie.Runtime <= 10)
            {
                return 90;
            }

            //30 minute episodes - 5 minutes
            if (movie.Runtime <= 30)
            {
                return 300;
            }

            //60 minute episodes - 10 minutes
            return 600;
        }
    }
}
