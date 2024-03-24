using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.MediaFiles.MovieImport.Aggregation;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Extras.Subtitles
{
    public class ExistingSubtitleImporter : ImportExistingExtraFilesBase<SubtitleFile>
    {
        private readonly IExtraFileService<SubtitleFile> _subtitleFileService;
        private readonly IAggregationService _aggregationService;
        private readonly IParsingService _parsingService;
        private readonly Logger _logger;

        public ExistingSubtitleImporter(IExtraFileService<SubtitleFile> subtitleFileService,
                                        IAggregationService aggregationService,
                                        IParsingService parsingService,
                                        Logger logger)
            : base(subtitleFileService)
        {
            _subtitleFileService = subtitleFileService;
            _aggregationService = aggregationService;
            _parsingService = parsingService;
            _logger = logger;
        }

        public override int Order => 1;

        public override IEnumerable<ExtraFile> ProcessFiles(Movie movie, List<string> filesOnDisk, List<string> importedFiles, string fileNameBeforeRename)
        {
            _logger.Debug("Looking for existing subtitle files in {0}", movie.Path);

            var subtitleFiles = new List<SubtitleFile>();
            var filterResult = FilterAndClean(movie, filesOnDisk, importedFiles, fileNameBeforeRename is not null);

            foreach (var possibleSubtitleFile in filterResult.FilesOnDisk)
            {
                var extension = Path.GetExtension(possibleSubtitleFile);

                if (SubtitleFileExtensions.Extensions.Contains(extension))
                {
                    var minimalInfo = _parsingService.ParseMinimalPathMovieInfo(possibleSubtitleFile);

                    if (minimalInfo == null)
                    {
                        _logger.Debug("Unable to parse subtitle file: {0}", possibleSubtitleFile);
                        continue;
                    }

                    var localMovie = new LocalMovie
                    {
                        FileMovieInfo = minimalInfo,
                        Movie = movie,
                        Path = possibleSubtitleFile,
                        FileNameBeforeRename = fileNameBeforeRename
                    };

                    try
                    {
                        _aggregationService.Augment(localMovie, null);
                    }
                    catch (AugmentingFailedException)
                    {
                        _logger.Debug("Unable to parse extra file: {0}", possibleSubtitleFile);
                        continue;
                    }

                    var subtitleFile = new SubtitleFile
                    {
                        MovieId = movie.Id,
                        MovieFileId = movie.MovieFileId,
                        RelativePath = movie.Path.GetRelativePath(possibleSubtitleFile),
                        Language = localMovie.SubtitleInfo.Language,
                        LanguageTags = localMovie.SubtitleInfo.LanguageTags,
                        Title = localMovie.SubtitleInfo.Title,
                        Extension = extension,
                        Copy = localMovie.SubtitleInfo.Copy
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
