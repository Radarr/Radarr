using System;
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
using NzbDrone.Core.Movies;

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

        public override IEnumerable<ExtraFile> CreateAfterMovieScan(Movie movie, List<MovieFile> movieFiles)
        {
            return Enumerable.Empty<SubtitleFile>();
        }

        public override IEnumerable<ExtraFile> CreateAfterMovieImport(Movie movie, MovieFile movieFile)
        {
            return Enumerable.Empty<SubtitleFile>();
        }

        public override IEnumerable<ExtraFile> CreateAfterMovieImport(Movie movie, string movieFolder)
        {
            return Enumerable.Empty<SubtitleFile>();
        }

        public override IEnumerable<ExtraFile> MoveFilesAfterRename(Movie movie, List<MovieFile> movieFiles)
        {
            var subtitleFiles = _subtitleFileService.GetFilesByMovie(movie.Id);

            var movedFiles = new List<SubtitleFile>();

            foreach (var movieFile in movieFiles)
            {
                var groupedExtraFilesForMovieFile = subtitleFiles.Where(m => m.MovieFileId == movieFile.Id)
                                                            .GroupBy(s => s.Language + s.Extension).ToList();

                foreach (var group in groupedExtraFilesForMovieFile)
                {
                    var groupCount = group.Count();
                    var copy = 1;

                    if (groupCount > 1)
                    {
                        _logger.Warn("Multiple subtitle files found with the same language and extension for {0}", Path.Combine(movie.Path, movieFile.RelativePath));
                    }

                    foreach (var subtitleFile in group)
                    {
                        var suffix = GetSuffix(subtitleFile.Language, copy, groupCount > 1);
                        movedFiles.AddIfNotNull(MoveFile(movie, movieFile, subtitleFile, suffix));

                        copy++;
                    }
                }
            }

            _subtitleFileService.Upsert(movedFiles);

            return movedFiles;
        }

        public override ExtraFile Import(Movie movie, MovieFile movieFile, string path, string extension, bool readOnly)
        {
            if (SubtitleFileExtensions.Extensions.Contains(Path.GetExtension(path)))
            {
                var language = LanguageParser.ParseSubtitleLanguage(path);
                var subtitleFiles = _subtitleFileService.GetFilesByMovie(movie.Id);
                var existingSrtSubs = subtitleFiles.Where(m => m.MovieFileId == movieFile.Id)
                    .Where(m => m.Language == language)
                    .Where(m => m.Extension == extension);

                var suffix = GetSuffix(language, existingSrtSubs.Count() + 1, extension.EqualsIgnoreCase(".srt"));                
                var subtitleFile = new SubtitleFile();
                
                if ((extension.EqualsIgnoreCase(".srt") && language != Language.Unknown) ||
                    !extension.EqualsIgnoreCase(".srt"))
                {
                    subtitleFile = ImportFile(movie, movieFile, path, readOnly, extension, suffix);
                    subtitleFile.Language = language;

                    _subtitleFileService.Upsert(subtitleFile);                                            
                }

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
