using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.MediaFiles.BookImport.Aggregation.Aggregators;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.BookImport.Aggregation
{
    public interface IAugmentingService
    {
        LocalBook Augment(LocalBook localTrack, bool otherFiles);
        LocalEdition Augment(LocalEdition localBook);
    }

    public class AugmentingService : IAugmentingService
    {
        private readonly IEnumerable<IAggregate<LocalBook>> _trackAugmenters;
        private readonly IEnumerable<IAggregate<LocalEdition>> _bookAugmenters;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public AugmentingService(IEnumerable<IAggregate<LocalBook>> trackAugmenters,
                                 IEnumerable<IAggregate<LocalEdition>> bookAugmenters,
                                 IDiskProvider diskProvider,
                                 Logger logger)
        {
            _trackAugmenters = trackAugmenters;
            _bookAugmenters = bookAugmenters;
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public LocalBook Augment(LocalBook localTrack, bool otherFiles)
        {
            if (localTrack.DownloadClientBookInfo == null &&
                localTrack.FolderTrackInfo == null &&
                localTrack.FileTrackInfo == null)
            {
                if (MediaFileExtensions.AllExtensions.Contains(Path.GetExtension(localTrack.Path)))
                {
                    throw new AugmentingFailedException("Unable to parse book info from path: {0}", localTrack.Path);
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

        public LocalEdition Augment(LocalEdition localBook)
        {
            foreach (var augmenter in _bookAugmenters)
            {
                try
                {
                    augmenter.Aggregate(localBook, false);
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, ex.Message);
                }
            }

            return localBook;
        }
    }
}
