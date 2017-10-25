using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Extras.Lyrics
{
    public class ExistingLyricImporter : ImportExistingExtraFilesBase<LyricFile>
    {
        private readonly IExtraFileService<LyricFile> _lyricFileService;
        private readonly IParsingService _parsingService;
        private readonly Logger _logger;

        public ExistingLyricImporter(IExtraFileService<LyricFile> lyricFileService,
                                        IParsingService parsingService,
                                        Logger logger)
            : base (lyricFileService)
        {
            _lyricFileService = lyricFileService;
            _parsingService = parsingService;
            _logger = logger;
        }

        public override int Order => 1;

        public override IEnumerable<ExtraFile> ProcessFiles(Artist artist, List<string> filesOnDisk, List<string> importedFiles)
        {
            _logger.Debug("Looking for existing lyrics files in {0}", artist.Path);

            var subtitleFiles = new List<LyricFile>();
            var filterResult = FilterAndClean(artist, filesOnDisk, importedFiles);

            foreach (var possibleSubtitleFile in filterResult.FilesOnDisk)
            {
                var extension = Path.GetExtension(possibleSubtitleFile);

                if (LyricFileExtensions.Extensions.Contains(extension))
                {
                    var localTrack = _parsingService.GetLocalTrack(possibleSubtitleFile, artist);

                    if (localTrack == null)
                    {
                        _logger.Debug("Unable to parse lyric file: {0}", possibleSubtitleFile);
                        continue;
                    }

                    if (localTrack.Tracks.Empty())
                    {
                        _logger.Debug("Cannot find related tracks for: {0}", possibleSubtitleFile);
                        continue;
                    }

                    if (localTrack.Tracks.DistinctBy(e => e.TrackFileId).Count() > 1)
                    {
                        _logger.Debug("Lyric file: {0} does not match existing files.", possibleSubtitleFile);
                        continue;
                    }

                    var subtitleFile = new LyricFile
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

            _logger.Info("Found {0} existing lyric files", subtitleFiles.Count);
            _lyricFileService.Upsert(subtitleFiles);

            // Return files that were just imported along with files that were
            // previously imported so previously imported files aren't imported twice

            return subtitleFiles.Concat(filterResult.PreviouslyImported);
        }
    }
}
