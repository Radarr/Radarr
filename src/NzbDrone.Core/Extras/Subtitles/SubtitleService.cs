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
using NzbDrone.Core.Tv;

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

        public override IEnumerable<ExtraFile> CreateAfterSeriesScan(Series series, List<EpisodeFile> episodeFiles)
        {
            return Enumerable.Empty<SubtitleFile>();
        }

        public override IEnumerable<ExtraFile> CreateAfterEpisodeImport(Series series, EpisodeFile episodeFile)
        {
            return Enumerable.Empty<SubtitleFile>();
        }

        public override IEnumerable<ExtraFile> CreateAfterEpisodeImport(Series series, string seriesFolder, string seasonFolder)
        {
            return Enumerable.Empty<SubtitleFile>();
        }

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

        public override IEnumerable<ExtraFile> MoveFilesAfterRename(Series series, List<EpisodeFile> episodeFiles)
        {
            var subtitleFiles = _subtitleFileService.GetFilesBySeries(series.Id);

            var movedFiles = new List<SubtitleFile>();

            foreach (var episodeFile in episodeFiles)
            {
                var groupedExtraFilesForEpisodeFile = subtitleFiles.Where(m => m.EpisodeFileId == episodeFile.Id)
                                                                   .GroupBy(s => s.Language + s.SpecialType + s.Extension).ToList();

                foreach (var group in groupedExtraFilesForEpisodeFile)
                {
                    var groupCount = group.Count();
                    var copy = 1;

                    if (groupCount > 1)
                    {
                        _logger.Warn("Multiple subtitle files found with the same language, special type, and extension for {0}", Path.Combine(series.Path, episodeFile.RelativePath));
                    }

                    foreach (var subtitleFile in group)
                    {
                        var suffix = GetSuffix(subtitleFile.Language, copy, groupCount > 1, subtitleFile.SpecialType);
                        movedFiles.AddIfNotNull(MoveFile(series, episodeFile, subtitleFile, suffix));

                        copy++;
                    }
                }
            }

            _subtitleFileService.Upsert(movedFiles);

            return movedFiles;
        }

        public override IEnumerable<ExtraFile> MoveFilesAfterRename(Movie movie, List<MovieFile> movieFiles)
        {
            var subtitleFiles = _subtitleFileService.GetFilesByMovie(movie.Id);

            var movedFiles = new List<SubtitleFile>();

            foreach (var movieFile in movieFiles)
            {
                var groupedExtraFilesForMovieFile = subtitleFiles.Where(m => m.MovieFileId == movieFile.Id)
                                                                 .GroupBy(s => s.Language + s.SpecialType + s.Extension).ToList();

                foreach (var group in groupedExtraFilesForMovieFile)
                {
                    var groupCount = group.Count();
                    var copy = 1;

                    if (groupCount > 1)
                    {
                        _logger.Warn("Multiple subtitle files found with the same language, special type, and extension for {0}", Path.Combine(movie.Path, movieFile.RelativePath));
                    }

                    foreach (var subtitleFile in group)
                    {
                        var suffix = GetSuffix(subtitleFile.Language, copy, groupCount > 1, subtitleFile.SpecialType);
                        movedFiles.AddIfNotNull(MoveFile(movie, movieFile, subtitleFile, suffix));

                        copy++;
                    }
                }
            }

            _subtitleFileService.Upsert(movedFiles);

            return movedFiles;
        }

        public override ExtraFile Import(Series series, EpisodeFile episodeFile, string path, string extension, bool readOnly)
        {
            if (SubtitleFileExtensions.Extensions.Contains(Path.GetExtension(path)))
            {
                string specialType;
                var language = LanguageParser.ParseSubtitleLanguage(path, out specialType);
                var suffix = GetSuffix(language, 1, false, specialType);
                var subtitleFile = ImportFile(series, episodeFile, path, readOnly, extension, suffix);
                subtitleFile.Language = language;
                subtitleFile.SpecialType = specialType;

                _subtitleFileService.Upsert(subtitleFile);

                return subtitleFile;
            }

            return null;
        }

        public override ExtraFile Import(Movie movie, MovieFile movieFile, string path, string extension, bool readOnly)
        {
            if (SubtitleFileExtensions.Extensions.Contains(Path.GetExtension(path)))
            {
                string specialType;
                var language = LanguageParser.ParseSubtitleLanguage(path, out specialType);
                var suffix = GetSuffix(language, 1, false, specialType);
                var subtitleFile = ImportFile(movie, movieFile, path, readOnly, extension, suffix);
                subtitleFile.Language = language;
                subtitleFile.SpecialType = specialType;

                _subtitleFileService.Upsert(subtitleFile);

                return subtitleFile;
            }

            return null;
        }

        private string GetSuffix(Language language, int copy, bool multipleCopies = false, string specialType = null)
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

            if (specialType.IsNotNullOrWhiteSpace())
            {
                suffixBuilder.Append(".");
                suffixBuilder.Append(specialType);
            }

            return suffixBuilder.ToString();
        }
    }
}
