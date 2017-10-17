using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Music;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Extras.Subtitles
{
    public class SubtitleService : ExtraFileManager<SubtitleFile>
    {
        private readonly ISubtitleFileService _subtitleFileService;
        private readonly Logger _logger;

        public SubtitleService(IConfigService configService,
                               IDiskProvider diskProvider,
                               IDiskTransferService diskTransferService,
                               ISubtitleFileService subtitleFileService,
                               Logger logger)
            : base(configService, diskProvider, diskTransferService, logger)
        {
            _subtitleFileService = subtitleFileService;
            _logger = logger;
        }

        public override int Order => 1;

        public override IEnumerable<ExtraFile> CreateAfterArtistScan(Artist artist, List<Album> albums, List<TrackFile> trackFiles)
        {
            return Enumerable.Empty<SubtitleFile>();
        }

        public override IEnumerable<ExtraFile> CreateAfterTrackImport(Artist artist, TrackFile trackFile)
        {
            return Enumerable.Empty<SubtitleFile>();
        }

        public override IEnumerable<ExtraFile> CreateAfterTrackImport(Artist artist, string artistFolder, string albumFolder)
        {
            return Enumerable.Empty<SubtitleFile>();
        }

        public override IEnumerable<ExtraFile> MoveFilesAfterRename(Artist artist, List<TrackFile> trackFiles)
        {
            var subtitleFiles = _subtitleFileService.GetFilesByArtist(artist.Id);

            var movedFiles = new List<SubtitleFile>();

            foreach (var trackFile in trackFiles)
            {
                var groupedExtraFilesForTrackFile = subtitleFiles.Where(m => m.TrackFileId == trackFile.Id)
                                                            .GroupBy(s => s.Language + s.Extension).ToList();

                foreach (var group in groupedExtraFilesForTrackFile)
                {
                    var groupCount = group.Count();
                    var copy = 1;

                    if (groupCount > 1)
                    {
                        _logger.Warn("Multiple subtitle files found with the same language and extension for {0}", Path.Combine(artist.Path, trackFile.RelativePath));
                    }

                    foreach (var subtitleFile in group)
                    {
                        var suffix = GetSuffix(subtitleFile.Language, copy, groupCount > 1);
                        movedFiles.AddIfNotNull(MoveFile(artist, trackFile, subtitleFile, suffix));

                        copy++;
                    }
                }
            }

            _subtitleFileService.Upsert(movedFiles);

            return movedFiles;
        }

        public override ExtraFile Import(Artist artist, TrackFile trackFile, string path, string extension, bool readOnly)
        {
            if (SubtitleFileExtensions.Extensions.Contains(Path.GetExtension(path)))
            {
                var language = LanguageParser.ParseSubtitleLanguage(path);
                var suffix = GetSuffix(language, 1, false);
                var subtitleFile = ImportFile(artist, trackFile, path, readOnly, extension, suffix);
                subtitleFile.Language = language;

                _subtitleFileService.Upsert(subtitleFile);

                return subtitleFile;
            }

            return null;
        }

        private string GetSuffix(Language language, int copy, bool multipleCopies = false)
        {
            var suffixBuilder = new StringBuilder();

            if (multipleCopies)
            {
                suffixBuilder.Append(".");
                suffixBuilder.Append(copy);
            }

            if (language != Language.Unknown)
            {
                suffixBuilder.Append(".");
                suffixBuilder.Append(IsoLanguages.Get(language).TwoLetterCode);
            }

            return suffixBuilder.ToString();
        }
    }
}
