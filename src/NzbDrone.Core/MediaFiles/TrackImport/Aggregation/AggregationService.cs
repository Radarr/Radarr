using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.MediaFiles.TrackImport.Aggregation.Aggregators;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.TrackImport.Aggregation
{
    public interface IAugmentingService
    {
        LocalTrack Augment(LocalTrack localTrack, bool otherFiles);
        LocalAlbumRelease Augment(LocalAlbumRelease localAlbum);
    }

    public class AugmentingService : IAugmentingService
    {
        private readonly IEnumerable<IAggregate<LocalTrack>> _trackAugmenters;
        private readonly IEnumerable<IAggregate<LocalAlbumRelease>> _albumAugmenters;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public AugmentingService(IEnumerable<IAggregate<LocalTrack>> trackAugmenters,
                                 IEnumerable<IAggregate<LocalAlbumRelease>> albumAugmenters,
                                 IDiskProvider diskProvider,
                                 Logger logger)
        {
            _trackAugmenters = trackAugmenters;
            _albumAugmenters = albumAugmenters;
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public LocalTrack Augment(LocalTrack localTrack, bool otherFiles)
        {
            if (localTrack.DownloadClientAlbumInfo == null &&
                localTrack.FolderTrackInfo == null &&
                localTrack.FileTrackInfo == null)
            {
                if (MediaFileExtensions.Extensions.Contains(Path.GetExtension(localTrack.Path)))
                {
                    throw new AugmentingFailedException("Unable to parse track info from path: {0}", localTrack.Path);
                }
            }

            localTrack.Size = _diskProvider.GetFileSize(localTrack.Path);

            foreach (var augmenter in _trackAugmenters)
            {
                try
                {
                    augmenter.Aggregate(localTrack, otherFiles);
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, ex.Message);
                }
            }

            return localTrack;
        }

        public LocalAlbumRelease Augment(LocalAlbumRelease localAlbum)
        {
            foreach (var augmenter in _albumAugmenters)
            {
                try
                {
                    augmenter.Aggregate(localAlbum, false);
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, ex.Message);
                }

            }

            return localAlbum;
        }
    }
}
