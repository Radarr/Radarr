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
        List<CustomFormat> ParseCustomFormat(RemoteMovie remoteMovie, long size);
        List<CustomFormat> ParseCustomFormat(MovieFile movieFile, Movie movie);
        List<CustomFormat> ParseCustomFormat(MovieFile movieFile);
        List<CustomFormat> ParseCustomFormat(Blocklist blocklist, Movie movie);
        List<CustomFormat> ParseCustomFormat(MovieHistory history, Movie movie);
        List<CustomFormat> ParseCustomFormat(LocalMovie localMovie);
    }

    public class CustomFormatCalculationService : ICustomFormatCalculationService
    {
        private readonly ICustomFormatService _formatService;

        public CustomFormatCalculationService(ICustomFormatService formatService)
        {
            _formatService = formatService;
        }

        public List<CustomFormat> ParseCustomFormat(RemoteMovie remoteMovie, long size)
        {
            var input = new CustomFormatInput
            {
                MovieInfo = remoteMovie.ParsedMovieInfo,
                Movie = remoteMovie.Movie,
                Size = size,
                Languages = remoteMovie.Languages,
                IndexerFlags = remoteMovie.Release?.IndexerFlags ?? 0
            };

            return ParseCustomFormat(input);
        }

        public List<CustomFormat> ParseCustomFormat(MovieFile movieFile, Movie movie)
        {
            return ParseCustomFormat(movieFile, movie, _formatService.All());
        }

        public List<CustomFormat> ParseCustomFormat(MovieFile movieFile)
        {
            return ParseCustomFormat(movieFile, movieFile.Movie, _formatService.All());
        }

        public List<CustomFormat> ParseCustomFormat(Blocklist blocklist, Movie movie)
        {
            var parsed = Parser.Parser.ParseMovieTitle(blocklist.SourceTitle);

            var movieInfo = new ParsedMovieInfo
            {
                MovieTitles = new List<string>() { movie.Title },
                SimpleReleaseTitle = parsed?.SimpleReleaseTitle ?? blocklist.SourceTitle.SimplifyReleaseTitle(),
                ReleaseTitle = parsed?.ReleaseTitle ?? blocklist.SourceTitle,
                Edition = parsed?.Edition,
                Quality = blocklist.Quality,
                Languages = blocklist.Languages,
                ReleaseGroup = parsed?.ReleaseGroup
            };

            var input = new CustomFormatInput
            {
                MovieInfo = movieInfo,
                Movie = movie,
                Size = blocklist.Size ?? 0,
                IndexerFlags = blocklist.IndexerFlags,
                Languages = blocklist.Languages
            };

            return ParseCustomFormat(input);
        }

        public List<CustomFormat> ParseCustomFormat(MovieHistory history, Movie movie)
        {
            var parsed = Parser.Parser.ParseMovieTitle(history.SourceTitle);

            long.TryParse(history.Data.GetValueOrDefault("size"), out var size);
            Enum.TryParse(history.Data.GetValueOrDefault("indexerFlags"), true, out IndexerFlags flags);

            var movieInfo = new ParsedMovieInfo
            {
                MovieTitles = new List<string>() { movie.Title },
                SimpleReleaseTitle = parsed?.SimpleReleaseTitle ?? history.SourceTitle.SimplifyReleaseTitle(),
                ReleaseTitle = parsed?.ReleaseTitle ?? history.SourceTitle,
                Edition = parsed?.Edition,
                Quality = history.Quality,
                Languages = history.Languages,
                ReleaseGroup = parsed?.ReleaseGroup,
            };

            var input = new CustomFormatInput
            {
                MovieInfo = movieInfo,
                Movie = movie,
                Size = size,
                IndexerFlags = flags,
                Languages = history.Languages
            };

            return ParseCustomFormat(input);
        }

        public List<CustomFormat> ParseCustomFormat(LocalMovie localMovie)
        {
            var episodeInfo = new ParsedMovieInfo
            {
                MovieTitles = new List<string>() { localMovie.Movie.Title },
                SimpleReleaseTitle = localMovie.SceneName?.SimplifyReleaseTitle(),
                ReleaseTitle = localMovie.SceneName,
                Quality = localMovie.Quality,
                Edition = localMovie.Edition,
                Languages = localMovie.Languages,
                ReleaseGroup = localMovie.ReleaseGroup
            };

            var input = new CustomFormatInput
            {
                MovieInfo = episodeInfo,
                Movie = localMovie.Movie,
                Size = localMovie.Size,
                Languages = localMovie.Languages
            };

            return ParseCustomFormat(input);
        }

        private List<CustomFormat> ParseCustomFormat(CustomFormatInput input)
        {
            return ParseCustomFormat(input, _formatService.All());
        }

        private static List<CustomFormat> ParseCustomFormat(CustomFormatInput input, List<CustomFormat> allCustomFormats)
        {
            var matches = new List<CustomFormat>();

            foreach (var customFormat in allCustomFormats)
            {
                var specificationMatches = customFormat.Specifications
                    .GroupBy(t => t.GetType())
                    .Select(g => new SpecificationMatchesGroup
                    {
                        Matches = g.ToDictionary(t => t, t => t.IsSatisfiedBy(input))
                    })
                    .ToList();

                if (specificationMatches.All(x => x.DidMatch))
                {
                    matches.Add(customFormat);
                }
            }

            return matches;
        }

        private static List<CustomFormat> ParseCustomFormat(MovieFile movieFile, Movie movie, List<CustomFormat> allCustomFormats)
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

            var movieInfo = new ParsedMovieInfo
            {
                MovieTitles = new List<string>() { movie.Title },
                SimpleReleaseTitle = sceneName.SimplifyReleaseTitle(),
                Quality = movieFile.Quality,
                Languages = movieFile.Languages,
                ReleaseGroup = movieFile.ReleaseGroup,
                Edition = movieFile.Edition,
                Year = movieFile.Movie.MovieMetadata.Value.Year,
                ImdbId = movieFile.Movie.MovieMetadata.Value.ImdbId
            };

            var input = new CustomFormatInput
            {
                MovieInfo = movieInfo,
                Movie = movie,
                Size = movieFile.Size,
                IndexerFlags = movieFile.IndexerFlags,
                Languages = movieFile.Languages,
                Filename = Path.GetFileName(movieFile.RelativePath)
            };

            return ParseCustomFormat(input, allCustomFormats);
        }
    }
}
