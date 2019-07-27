using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using System.Linq;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.MediaFiles.MediaInfo
{
    public interface IUpdateMediaInfo
    {
        void Update(MovieFile movieFile, Movie movie);
    }

    public class UpdateMediaInfoService : IHandle<MovieScannedEvent>, IUpdateMediaInfo
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IMediaFileService _mediaFileService;
        private readonly IVideoFileInfoReader _videoFileInfoReader;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public UpdateMediaInfoService(IDiskProvider diskProvider,
                                IMediaFileService mediaFileService,
                                IVideoFileInfoReader videoFileInfoReader,
                                IConfigService configService,
                                Logger logger)
        {
            _diskProvider = diskProvider;
            _mediaFileService = mediaFileService;
            _videoFileInfoReader = videoFileInfoReader;
            _configService = configService;
            _logger = logger;
        }

        public void Handle(MovieScannedEvent message)
        {
            if (!_configService.EnableMediaInfo)
            {
                _logger.Debug("MediaInfo is disabled");
                return;
            }

            var allMediaFiles = _mediaFileService.GetFilesByMovie(message.Movie.Id);
            var filteredMediaFiles = allMediaFiles.Where(c =>
                c.MediaInfo == null ||
                c.MediaInfo.SchemaRevision < VideoFileInfoReader.MINIMUM_MEDIA_INFO_SCHEMA_REVISION).ToList();

            foreach (var mediaFile in filteredMediaFiles)
            {
                UpdateMediaInfo(mediaFile, message.Movie);
            }
        }

        public void Update(MovieFile movieFile, Movie movie)
        {
            if (!_configService.EnableMediaInfo)
            {
                _logger.Debug("MediaInfo is disabled");
                return;
            }

            UpdateMediaInfo(movieFile, movie);
        }

        private void UpdateMediaInfo(MovieFile movieFile, Movie movie)
        {
            var path = Path.Combine(movie.Path, movieFile.RelativePath);

            if (!_diskProvider.FileExists(path))
            {
                _logger.Debug("Can't update MediaInfo because '{0}' does not exist", path);
                return;
            }

            var updatedMediaInfo = _videoFileInfoReader.GetMediaInfo(path);

            if (updatedMediaInfo != null)
            {
                movieFile.MediaInfo = updatedMediaInfo;
                _mediaFileService.Update(movieFile);
                _logger.Debug("Updated MediaInfo for '{0}'", path);
            }
        }
    }
}
