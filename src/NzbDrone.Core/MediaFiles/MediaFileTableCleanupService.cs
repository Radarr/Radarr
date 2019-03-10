using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.MediaFiles
{
    public interface IMediaFileTableCleanupService
    {
        void Clean(Artist artist, List<string> filesOnDisk);
    }

    public class MediaFileTableCleanupService : IMediaFileTableCleanupService
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly ITrackService _trackService;
        private readonly Logger _logger;

        public MediaFileTableCleanupService(IMediaFileService mediaFileService,
                                            ITrackService trackService,
                                            Logger logger)
        {
            _mediaFileService = mediaFileService;
            _trackService = trackService;
            _logger = logger;
        }

        public void Clean(Artist artist, List<string> filesOnDisk)
        {
            var artistFiles = _mediaFileService.GetFilesByArtist(artist.Id);
            var tracks = _trackService.GetTracksByArtist(artist.Id);


            var filesOnDiskKeys = new HashSet<string>(filesOnDisk, PathEqualityComparer.Instance);
            
            foreach (var artistFile in artistFiles)
            {
                var trackFile = artistFile;
                var trackFilePath = Path.Combine(artist.Path, trackFile.RelativePath);

                try
                {
                    if (!filesOnDiskKeys.Contains(trackFilePath))
                    {
                        _logger.Debug("File [{0}] no longer exists on disk, removing from db", trackFilePath);
                        _mediaFileService.Delete(artistFile, DeleteMediaFileReason.MissingFromDisk);
                        continue;
                    }

                    if (tracks.None(e => e.TrackFileId == trackFile.Id))
                    {
                        _logger.Debug("File [{0}] is not assigned to any artist, removing from db", trackFilePath);
                        _mediaFileService.Delete(trackFile, DeleteMediaFileReason.NoLinkedEpisodes);
                        continue;
                    }
                }

                catch (Exception ex)
                {
                    _logger.Error(ex, "Unable to cleanup TrackFile in DB: {0}", trackFile.Id);
                }
            }

            foreach (var t in tracks)
            {
                var track = t;

                if (track.TrackFileId > 0 && artistFiles.None(f => f.Id == track.TrackFileId))
                {
                    track.TrackFileId = 0;
                    _trackService.UpdateTrack(track);
                }
            }
        }
    }
}
