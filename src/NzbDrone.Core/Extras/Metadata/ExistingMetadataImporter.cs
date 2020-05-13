using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.MediaFiles.BookImport.Aggregation;
using NzbDrone.Core.Parser;
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

        public override IEnumerable<ExtraFile> ProcessFiles(Author author, List<string> filesOnDisk, List<string> importedFiles)
        {
            _logger.Debug("Looking for existing metadata in {0}", author.Path);

            var metadataFiles = new List<MetadataFile>();
            var filterResult = FilterAndClean(author, filesOnDisk, importedFiles);

            foreach (var possibleMetadataFile in filterResult.FilesOnDisk)
            {
                foreach (var consumer in _consumers)
                {
                    var metadata = consumer.FindMetadataFile(author, possibleMetadataFile);

                    if (metadata == null)
                    {
                        continue;
                    }

                    if (metadata.Type == MetadataType.AlbumImage || metadata.Type == MetadataType.AlbumMetadata)
                    {
                        var localAlbum = _parsingService.GetLocalAlbum(possibleMetadataFile, author);

                        if (localAlbum == null)
                        {
                            _logger.Debug("Extra file folder has multiple Albums: {0}", possibleMetadataFile);
                            continue;
                        }

                        metadata.BookId = localAlbum.Id;
                    }

                    if (metadata.Type == MetadataType.TrackMetadata)
                    {
                        var localTrack = new LocalBook
                        {
                            FileTrackInfo = Parser.Parser.ParseMusicPath(possibleMetadataFile),
                            Author = author,
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

                        if (localTrack.Book == null)
                        {
                            _logger.Debug("Cannot find related book for: {0}", possibleMetadataFile);
                            continue;
                        }
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
