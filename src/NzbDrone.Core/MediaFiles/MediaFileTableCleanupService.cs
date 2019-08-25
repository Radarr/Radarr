using System.Collections.Generic;
using System.Linq;
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
            var dbFiles = _mediaFileService.GetFilesWithBasePath(artist.Path);

            // get files in database that are missing on disk and remove from database
            var missingFiles = dbFiles.ExceptBy(x => x.Path, filesOnDisk, x => x, PathEqualityComparer.Instance).ToList();

            _logger.Debug("The following files no longer exist on disk, removing from db:\n{0}",
                          string.Join("\n", missingFiles.Select(x => x.Path)));

            _mediaFileService.DeleteMany(missingFiles, DeleteMediaFileReason.MissingFromDisk);

            // get any tracks matched to these trackfiles and unlink them
            var orphanedTracks = _trackService.GetTracksByFileId(missingFiles.Select(x => x.Id));
            orphanedTracks.ForEach(x => x.TrackFileId = 0);
            _trackService.SetFileIds(orphanedTracks);
        }
    }
}
