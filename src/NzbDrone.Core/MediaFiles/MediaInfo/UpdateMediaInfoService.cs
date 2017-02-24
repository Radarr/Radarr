﻿using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.MediaFiles.MediaInfo
{
    public class UpdateMediaInfoService : IHandle<MovieScannedEvent>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IMediaFileService _mediaFileService;
        private readonly IVideoFileInfoReader _videoFileInfoReader;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        private const int CURRENT_MEDIA_INFO_SCHEMA_REVISION = 3;

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

        private void UpdateMediaInfo(Movie series, List<MovieFile> mediaFiles)
        {
            foreach (var mediaFile in mediaFiles)
            {
                var path = Path.Combine(series.Path, mediaFile.RelativePath);

                if (!_diskProvider.FileExists(path))
                {
                    _logger.Debug("Can't update MediaInfo because '{0}' does not exist", path);
                    continue;
                }

                mediaFile.MediaInfo = _videoFileInfoReader.GetMediaInfo(path);

                if (mediaFile.MediaInfo != null)
                {
                    mediaFile.MediaInfo.SchemaRevision = CURRENT_MEDIA_INFO_SCHEMA_REVISION;
                    _mediaFileService.Update(mediaFile);
                    _logger.Debug("Updated MediaInfo for '{0}'", path);
                }
            }
        }

        public void Handle(MovieScannedEvent message)
        {
            if (!_configService.EnableMediaInfo)
            {
                _logger.Debug("MediaInfo is disabled");
                return;
            }

            var allMediaFiles = _mediaFileService.GetFilesByMovie(message.Movie.Id);
            var filteredMediaFiles = allMediaFiles.Where(c => c.MediaInfo == null || c.MediaInfo.SchemaRevision < CURRENT_MEDIA_INFO_SCHEMA_REVISION).ToList();

            UpdateMediaInfo(message.Movie, filteredMediaFiles);
        }
    }
}
