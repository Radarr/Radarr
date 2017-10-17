using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Extras.Subtitles
{
    public class ExistingSubtitleImporter : ImportExistingExtraFilesBase<SubtitleFile>
    {
        private readonly IExtraFileService<SubtitleFile> _subtitleFileService;
        private readonly IParsingService _parsingService;
        private readonly Logger _logger;

        public ExistingSubtitleImporter(IExtraFileService<SubtitleFile> subtitleFileService,
                                        IParsingService parsingService,
                                        Logger logger)
            : base (subtitleFileService)
        {
            _subtitleFileService = subtitleFileService;
            _parsingService = parsingService;
            _logger = logger;
        }

        public override int Order => 1;

        public override IEnumerable<ExtraFile> ProcessFiles(Artist artist, List<string> filesOnDisk, List<string> importedFiles)
        {
            _logger.Debug("Looking for existing subtitle files in {0}", artist.Path);

            var subtitleFiles = new List<SubtitleFile>();
            var filterResult = FilterAndClean(artist, filesOnDisk, importedFiles);

            foreach (var possibleSubtitleFile in filterResult.FilesOnDisk)
            {
                var extension = Path.GetExtension(possibleSubtitleFile);

                if (SubtitleFileExtensions.Extensions.Contains(extension))
                {
                    var localTrack = _parsingService.GetLocalTrack(possibleSubtitleFile, artist);

                    if (localTrack == null)
                    {
                        _logger.Debug("Unable to parse subtitle file: {0}", possibleSubtitleFile);
                        continue;
                    }

                    if (localTrack.Tracks.Empty())
                    {
                        _logger.Debug("Cannot find related tracks for: {0}", possibleSubtitleFile);
                        continue;
                    }

                    if (localTrack.Tracks.DistinctBy(e => e.TrackFileId).Count() > 1)
                    {
                        _logger.Debug("Subtitle file: {0} does not match existing files.", possibleSubtitleFile);
                        continue;
                    }

                    var subtitleFile = new SubtitleFile
                                       {
                                           ArtistId = artist.Id,
                                           AlbumId = localTrack.Album.Id,
                                           TrackFileId = localTrack.Tracks.First().TrackFileId,
                                           RelativePath = artist.Path.GetRelativePath(possibleSubtitleFile),
                                           Language = LanguageParser.ParseSubtitleLanguage(possibleSubtitleFile),
                                           Extension = extension
                                       };

                    subtitleFiles.Add(subtitleFile);
                }
            }

            _logger.Info("Found {0} existing subtitle files", subtitleFiles.Count);
            _subtitleFileService.Upsert(subtitleFiles);

            // Return files that were just imported along with files that were
            // previously imported so previously imported files aren't imported twice

            return subtitleFiles.Concat(filterResult.PreviouslyImported);
        }
    }
}
