using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.Extras.Lyrics;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Music;
using NzbDrone.Core.MediaFiles.TrackImport.Aggregation;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Extras.Metadata
{
    public class ExistingMetadataImporter : ImportExistingExtraFilesBase<MetadataFile>
    {
        private readonly IExtraFileService<MetadataFile> _metadataFileService;
        private readonly IParsingService _parsingService;
        private readonly IAugmentingService _augmentingService;
        private readonly Logger _logger;
        private readonly List<IMetadata> _consumers;

        public ExistingMetadataImporter(IExtraFileService<MetadataFile> metadataFileService,
                                        IEnumerable<IMetadata> consumers,
                                        IParsingService parsingService,
                                        IAugmentingService augmentingService,
                                        Logger logger)
        : base(metadataFileService)
        {
            _metadataFileService = metadataFileService;
            _parsingService = parsingService;
            _augmentingService = augmentingService;
            _logger = logger;
            _consumers = consumers.ToList();
        }

        public override int Order => 0;

        public override IEnumerable<ExtraFile> ProcessFiles(Artist artist, List<string> filesOnDisk, List<string> importedFiles)
        {
            _logger.Debug("Looking for existing metadata in {0}", artist.Path);

            var metadataFiles = new List<MetadataFile>();
            var filterResult = FilterAndClean(artist, filesOnDisk, importedFiles);

            foreach (var possibleMetadataFile in filterResult.FilesOnDisk)
            {
                // Don't process files that have known Subtitle file extensions (saves a bit of unecessary processing)

                if (LyricFileExtensions.Extensions.Contains(Path.GetExtension(possibleMetadataFile)))
                {
                    continue;
                }

                foreach (var consumer in _consumers)
                {
                    var metadata = consumer.FindMetadataFile(artist, possibleMetadataFile);

                    if (metadata == null)
                    {
                        continue;
                    }

                    if (metadata.Type == MetadataType.AlbumImage || metadata.Type == MetadataType.AlbumMetadata)
                    {
                        var localAlbum = _parsingService.GetLocalAlbum(possibleMetadataFile, artist);

                        if (localAlbum == null)
                        {
                            _logger.Debug("Extra file folder has multiple Albums: {0}", possibleMetadataFile);
                            continue;
                        }

                        metadata.AlbumId = localAlbum.Id;
                    }

                    if (metadata.Type == MetadataType.TrackMetadata)
                    {
                        var localTrack = new LocalTrack
                        {
                            FileTrackInfo = Parser.Parser.ParseMusicPath(possibleMetadataFile),
                            Artist = artist,
                            Path = possibleMetadataFile
                        };
                
                        try
                        {
                            _augmentingService.Augment(localTrack, false);
                        }
                        catch (AugmentingFailedException)
                        {
                            _logger.Debug("Unable to parse extra file: {0}", possibleMetadataFile);
                            continue;
                        }

                        if (localTrack.Tracks.Empty())
                        {
                            _logger.Debug("Cannot find related tracks for: {0}", possibleMetadataFile);
                            continue;
                        }

                        if (localTrack.Tracks.DistinctBy(e => e.TrackFileId).Count() > 1)
                        {
                            _logger.Debug("Extra file: {0} does not match existing files.", possibleMetadataFile);
                            continue;
                        }
                        
                        metadata.TrackFileId = localTrack.Tracks.First().TrackFileId;
                    }

                    metadata.Extension = Path.GetExtension(possibleMetadataFile);

                    metadataFiles.Add(metadata);
                }
            }

            _logger.Info("Found {0} existing metadata files", metadataFiles.Count);
            _metadataFileService.Upsert(metadataFiles);

            // Return files that were just imported along with files that were
            // previously imported so previously imported files aren't imported twice

            return metadataFiles.Concat(filterResult.PreviouslyImported);
        }
    }
}
