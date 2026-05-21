using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport.Aggregation
{
    public interface IAggregationService
    {
        LocalMovie Augment(LocalMovie localMovie, DownloadClientItem downloadClientItem);
    }

    public class AggregationService : IAggregationService
    {
        private readonly IEnumerable<IAggregateLocalMovie> _augmenters;
        private readonly IDiskProvider _diskProvider;
        private readonly IVideoFileInfoReader _videoFileInfoReader;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public AggregationService(IEnumerable<IAggregateLocalMovie> augmenters,
                                 IDiskProvider diskProvider,
                                 IVideoFileInfoReader videoFileInfoReader,
                                 IConfigService configService,
                                 Logger logger)
        {
            _augmenters = augmenters.OrderBy(a => a.Order).ToList();
            _diskProvider = diskProvider;
            _videoFileInfoReader = videoFileInfoReader;
            _configService = configService;
            _logger = logger;
        }

        public LocalMovie Augment(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            var isMediaFile = MediaFileExtensions.Extensions.Contains(Path.GetExtension(localMovie.Path));

            if (localMovie.DownloadClientMovieInfo == null &&
                localMovie.FolderMovieInfo == null &&
                localMovie.FileMovieInfo == null)
            {
                if (isMediaFile)
                {
                    throw new AugmentingFailedException("Unable to parse movie info from path: {0}", localMovie.Path);
                }
            }

            localMovie.Size = _diskProvider.GetFileSize(localMovie.Path);
            localMovie.SceneName = localMovie.SceneSource ? SceneNameCalculator.GetSceneName(localMovie) : null;

            if (isMediaFile && (!localMovie.ExistingFile || _configService.EnableMediaInfo))
            {
                localMovie.MediaInfo = _videoFileInfoReader.GetMediaInfo(localMovie.Path);
            }

            foreach (var augmenter in _augmenters)
            {
                try
                {
                    augmenter.Aggregate(localMovie, downloadClientItem);
                }
                catch (Exception ex)
                {
                    var message = $"Unable to augment information for file: '{localMovie.Path}'. Movie: {localMovie.Movie} Error: {ex.Message}";

                    _logger.Warn(ex, message);
                }
            }

            return localMovie;
        }
    }
}
