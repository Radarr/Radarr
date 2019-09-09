using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.MediaFiles.TrackImport.Aggregation;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Music;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Extras.Others
{
    public class ExistingOtherExtraImporter : ImportExistingExtraFilesBase<OtherExtraFile>
    {
        private readonly IExtraFileService<OtherExtraFile> _otherExtraFileService;
        private readonly IAugmentingService _augmentingService;
        private readonly Logger _logger;

        public ExistingOtherExtraImporter(IExtraFileService<OtherExtraFile> otherExtraFileService,
                                          IAugmentingService augmentingService,
                                          Logger logger)
            : base(otherExtraFileService)
        {
            _otherExtraFileService = otherExtraFileService;
            _augmentingService = augmentingService;
            _logger = logger;
        }

        public override int Order => 2;

        public override IEnumerable<ExtraFile> ProcessFiles(Artist artist, List<string> filesOnDisk, List<string> importedFiles)
        {
            _logger.Debug("Looking for existing extra files in {0}", artist.Path);

            var extraFiles = new List<OtherExtraFile>();
            var filterResult = FilterAndClean(artist, filesOnDisk, importedFiles);

            foreach (var possibleExtraFile in filterResult.FilesOnDisk)
            {
                var extension = Path.GetExtension(possibleExtraFile);

                if (extension.IsNullOrWhiteSpace())
                {
                    _logger.Debug("No extension for file: {0}", possibleExtraFile);
                    continue;
                }

                var localTrack = new LocalTrack
                {
                    FileTrackInfo = Parser.Parser.ParseMusicPath(possibleExtraFile),
                    Artist = artist,
                    Path = possibleExtraFile
                };
                
                try
                {
                    _augmentingService.Augment(localTrack, false);
                }
                catch (AugmentingFailedException)
                {
                    _logger.Debug("Unable to parse extra file: {0}", possibleExtraFile);
                    continue;
                }

                if (localTrack.Tracks.Empty())
                {
                    _logger.Debug("Cannot find related tracks for: {0}", possibleExtraFile);
                    continue;
                }

                if (localTrack.Tracks.DistinctBy(e => e.TrackFileId).Count() > 1)
                {
                    _logger.Debug("Extra file: {0} does not match existing files.", possibleExtraFile);
                    continue;
                }

                var extraFile = new OtherExtraFile
                {
                    ArtistId = artist.Id,
                    AlbumId = localTrack.Album.Id,
                    TrackFileId = localTrack.Tracks.First().TrackFileId,
                    RelativePath = artist.Path.GetRelativePath(possibleExtraFile),
                    Extension = extension
                };

                extraFiles.Add(extraFile);
            }

            _logger.Info("Found {0} existing other extra files", extraFiles.Count);
            _otherExtraFileService.Upsert(extraFiles);

            // Return files that were just imported along with files that were
            // previously imported so previously imported files aren't imported twice

            return extraFiles.Concat(filterResult.PreviouslyImported);
        }
    }
}
