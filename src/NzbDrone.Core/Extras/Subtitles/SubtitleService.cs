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
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MovieImport;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Extras.Subtitles
{
    public class SubtitleService : ExtraFileManager<SubtitleFile>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IDetectSample _detectSample;
        private readonly ISubtitleFileService _subtitleFileService;
        private readonly IMediaFileAttributeService _mediaFileAttributeService;
        private readonly Logger _logger;

        public SubtitleService(IConfigService configService,
                               IDiskProvider diskProvider,
                               IDiskTransferService diskTransferService,
                               IDetectSample detectSample,
                               ISubtitleFileService subtitleFileService,
                               IMediaFileAttributeService mediaFileAttributeService,
                               Logger logger)
            : base(configService, diskProvider, diskTransferService, logger)
        {
            _diskProvider = diskProvider;
            _detectSample = detectSample;
            _subtitleFileService = subtitleFileService;
            _mediaFileAttributeService = mediaFileAttributeService;
            _logger = logger;
        }

        public override int Order => 1;

        public override IEnumerable<ExtraFile> CreateAfterMediaCoverUpdate(Movie movie)
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

        public override IEnumerable<ExtraFile> CreateAfterMovieFolder(Movie movie, string movieFolder)
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
                                                            .GroupBy(s => s.AggregateString).ToList();

                foreach (var group in groupedExtraFilesForMovieFile)
                {
                    var multipleCopies = group.Count() > 1;
                    var orderedGroup = group.OrderBy(s => -s.Copy).ToList();
                    var copy = group.First().Copy;

                    foreach (var subtitleFile in orderedGroup)
                    {
                        if (multipleCopies && subtitleFile.Copy == 0)
                        {
                            subtitleFile.Copy = ++copy;
                        }

                        var suffix = GetSuffix(subtitleFile.Language, subtitleFile.Copy, subtitleFile.LanguageTags, multipleCopies, subtitleFile.Title);

                        movedFiles.AddIfNotNull(MoveFile(movie, movieFile, subtitleFile, suffix));

                        copy++;
                    }
                }
            }

            _subtitleFileService.Upsert(movedFiles);

            return movedFiles;
        }

        public override bool CanImportFile(LocalMovie localEpisode, MovieFile movieFile, string path, string extension, bool readOnly)
        {
            return SubtitleFileExtensions.Extensions.Contains(extension.ToLowerInvariant());
        }

        public override IEnumerable<ExtraFile> ImportFiles(LocalMovie localMovie, MovieFile movieFile, List<string> files, bool isReadOnly)
        {
            var importedFiles = new List<SubtitleFile>();

            var filteredFiles = files.Where(f => CanImportFile(localMovie, movieFile, f, Path.GetExtension(f), isReadOnly)).ToList();

            var sourcePath = localMovie.Path;
            var sourceFolder = _diskProvider.GetParentFolder(sourcePath);
            var sourceFileName = Path.GetFileNameWithoutExtension(sourcePath);

            var matchingFiles = new List<string>();

            foreach (var file in filteredFiles)
            {
                try
                {
                    // Filename match
                    if (Path.GetFileNameWithoutExtension(file).StartsWithIgnoreCase(sourceFileName))
                    {
                        matchingFiles.Add(file);
                        continue;
                    }

                    // Movie match
                    var fileMovieInfo = Parser.Parser.ParseMoviePath(file) ?? new ParsedMovieInfo();

                    if (fileMovieInfo.MovieTitle == null)
                    {
                        continue;
                    }

                    if (fileMovieInfo.MovieTitle == localMovie.FileMovieInfo.MovieTitle &&
                        fileMovieInfo.Year.Equals(localMovie.FileMovieInfo.Year))
                    {
                        matchingFiles.Add(file);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Failed to import subtitle file: {0}", file);
                }
            }

            // Use any sub if only episode in folder
            if (matchingFiles.Count == 0 && filteredFiles.Count > 0)
            {
                var videoFiles = _diskProvider.GetFiles(sourceFolder, true)
                                              .Where(file => MediaFileExtensions.Extensions.Contains(Path.GetExtension(file)))
                                              .ToList();

                if (videoFiles.Count > 2)
                {
                    return importedFiles;
                }

                // Filter out samples
                videoFiles = videoFiles.Where(file =>
                {
                    var sample = _detectSample.IsSample(localMovie.Movie.MovieMetadata, file);

                    if (sample == DetectSampleResult.Sample)
                    {
                        return false;
                    }

                    return true;
                }).ToList();

                if (videoFiles.Count == 1)
                {
                    matchingFiles.AddRange(filteredFiles);

                    _logger.Warn("Imported any available subtitle file for movie: {0}", localMovie);
                }
            }

            var subtitleFiles = new List<SubtitleFile>();

            foreach (var file in matchingFiles)
            {
                var language = LanguageParser.ParseSubtitleLanguage(file);
                var extension = Path.GetExtension(file);
                var languageTags = LanguageParser.ParseLanguageTags(file);
                var subFile = new SubtitleFile
                {
                    Language = language,
                    Extension = extension,
                    LanguageTags = languageTags
                };
                subFile.RelativePath = PathExtensions.GetRelativePath(sourceFolder, file);
                subtitleFiles.Add(subFile);
            }

            var groupedSubtitleFiles = subtitleFiles.GroupBy(s => s.AggregateString).ToList();

            foreach (var group in groupedSubtitleFiles)
            {
                var groupCount = group.Count();
                var copy = 1;

                foreach (var file in group)
                {
                    var path = Path.Combine(sourceFolder, file.RelativePath);
                    var language = file.Language;
                    var extension = file.Extension;
                    var suffix = GetSuffix(language, copy, file.LanguageTags, groupCount > 1);
                    try
                    {
                        var subtitleFile = ImportFile(localMovie.Movie, movieFile, path, isReadOnly, extension, suffix);
                        subtitleFile.Language = language;
                        subtitleFile.LanguageTags = file.LanguageTags;

                        _mediaFileAttributeService.SetFilePermissions(path);
                        _subtitleFileService.Upsert(subtitleFile);

                        importedFiles.Add(subtitleFile);

                        copy++;
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn(ex, "Failed to import subtitle file: {0}", path);
                    }
                }
            }

            return importedFiles;
        }

        private string GetSuffix(Language language, int copy, List<string> languageTags, bool multipleCopies = false, string title = null)
        {
            var suffixBuilder = new StringBuilder();

            if (title is not null)
            {
                suffixBuilder.Append('.');
                suffixBuilder.Append(title);

                if (multipleCopies)
                {
                    suffixBuilder.Append(" - ");
                    suffixBuilder.Append(copy);
                }
            }
            else if (multipleCopies)
            {
                suffixBuilder.Append('.');
                suffixBuilder.Append(copy);
            }

            if (language != Language.Unknown)
            {
                suffixBuilder.Append('.');
                suffixBuilder.Append(IsoLanguages.Get(language).TwoLetterCode);
            }

            if (languageTags.Any())
            {
                suffixBuilder.Append('.');
                suffixBuilder.Append(string.Join(".", languageTags));
            }

            return suffixBuilder.ToString();
        }
    }
}
