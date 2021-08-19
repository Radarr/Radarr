using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Blocklisting;
using NzbDrone.Core.History;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.CustomFormats
{
    public interface ICustomFormatCalculationService
    {
        List<CustomFormat> ParseCustomFormat(ParsedMovieInfo movieInfo);
        List<CustomFormat> ParseCustomFormat(MovieFile movieFile);
        List<CustomFormat> ParseCustomFormat(Blocklist blocklist);
        List<CustomFormat> ParseCustomFormat(MovieHistory history);
    }

    public class CustomFormatCalculationService : ICustomFormatCalculationService
    {
        private readonly ICustomFormatService _formatService;
        private readonly IParsingService _parsingService;
        private readonly IMovieService _movieService;

        public CustomFormatCalculationService(ICustomFormatService formatService,
                                              IParsingService parsingService,
                                              IMovieService movieService)
        {
            _formatService = formatService;
            _parsingService = parsingService;
            _movieService = movieService;
        }

        public static List<CustomFormat> ParseCustomFormat(ParsedMovieInfo movieInfo, List<CustomFormat> allCustomFormats)
        {
            var matches = new List<CustomFormat>();

            foreach (var customFormat in allCustomFormats)
            {
                var specificationMatches = customFormat.Specifications
                    .GroupBy(t => t.GetType())
                    .Select(g => new SpecificationMatchesGroup
                    {
                        Matches = g.ToDictionary(t => t, t => t.IsSatisfiedBy(movieInfo))
                    })
                    .ToList();

                if (specificationMatches.All(x => x.DidMatch))
                {
                    matches.Add(customFormat);
                }
            }

            return matches;
        }

        public static List<CustomFormat> ParseCustomFormat(MovieFile movieFile, List<CustomFormat> allCustomFormats)
        {
            var sceneName = string.Empty;
            if (movieFile.SceneName.IsNotNullOrWhiteSpace())
            {
                sceneName = movieFile.SceneName;
            }
            else if (movieFile.OriginalFilePath.IsNotNullOrWhiteSpace())
            {
                sceneName = movieFile.OriginalFilePath;
            }
            else if (movieFile.RelativePath.IsNotNullOrWhiteSpace())
            {
                sceneName = Path.GetFileName(movieFile.RelativePath);
            }

            var info = new ParsedMovieInfo
            {
                MovieTitle = movieFile.Movie.Title,
                SimpleReleaseTitle = sceneName.SimplifyReleaseTitle(),
                Quality = movieFile.Quality,
                Languages = movieFile.Languages,
                ReleaseGroup = movieFile.ReleaseGroup,
                Edition = movieFile.Edition,
                Year = movieFile.Movie.Year,
                ImdbId = movieFile.Movie.ImdbId,
                ExtraInfo = new Dictionary<string, object>
                {
                    { "IndexerFlags", movieFile.IndexerFlags },
                    { "Size", movieFile.Size },
                    { "Filename", System.IO.Path.GetFileName(movieFile.RelativePath) }
                }
            };

            return ParseCustomFormat(info, allCustomFormats);
        }

        public List<CustomFormat> ParseCustomFormat(ParsedMovieInfo movieInfo)
        {
            return ParseCustomFormat(movieInfo, _formatService.All());
        }

        public List<CustomFormat> ParseCustomFormat(MovieFile movieFile)
        {
            return ParseCustomFormat(movieFile, _formatService.All());
        }

        public List<CustomFormat> ParseCustomFormat(Blocklist blocklist)
        {
            var movie = _movieService.GetMovie(blocklist.MovieId);
            var parsed = _parsingService.ParseMovieInfo(blocklist.SourceTitle, null);

            var info = new ParsedMovieInfo
            {
                MovieTitle = movie.Title,
                SimpleReleaseTitle = parsed?.SimpleReleaseTitle ?? blocklist.SourceTitle.SimplifyReleaseTitle(),
                Quality = blocklist.Quality,
                Languages = blocklist.Languages,
                ReleaseGroup = parsed?.ReleaseGroup,
                Edition = parsed?.Edition,
                Year = movie.Year,
                ImdbId = movie.ImdbId,
                ExtraInfo = new Dictionary<string, object>
                {
                    { "IndexerFlags", blocklist.IndexerFlags },
                    { "Size", blocklist.Size }
                }
            };

            return ParseCustomFormat(info);
        }

        public List<CustomFormat> ParseCustomFormat(MovieHistory history)
        {
            var movie = _movieService.GetMovie(history.MovieId);
            var parsed = _parsingService.ParseMovieInfo(history.SourceTitle, null);

            Enum.TryParse(history.Data.GetValueOrDefault("indexerFlags"), true, out IndexerFlags flags);
            long.TryParse(history.Data.GetValueOrDefault("size"), out var size);

            var info = new ParsedMovieInfo
            {
                MovieTitle = movie.Title,
                SimpleReleaseTitle = parsed?.SimpleReleaseTitle ?? history.SourceTitle.SimplifyReleaseTitle(),
                Quality = history.Quality,
                Languages = history.Languages,
                ReleaseGroup = parsed?.ReleaseGroup,
                Edition = parsed?.Edition,
                Year = movie.Year,
                ImdbId = movie.ImdbId,
                ExtraInfo = new Dictionary<string, object>
                {
                    { "IndexerFlags", flags },
                    { "Size", size }
                }
            };

            return ParseCustomFormat(info);
        }
    }
}
