using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
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
        private readonly Logger _logger;

        public CustomFormatCalculationService(ICustomFormatService formatService, Logger logger)
        {
            _formatService = formatService;
            _logger = logger;
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
                Languages = blocklist.Languages,
                IndexerFlags = blocklist.IndexerFlags
            };

            return ParseCustomFormat(input);
        }

        public List<CustomFormat> ParseCustomFormat(MovieHistory history, Movie movie)
        {
            var parsed = Parser.Parser.ParseMovieTitle(history.SourceTitle);

            long.TryParse(history.Data.GetValueOrDefault("size"), out var size);
            Enum.TryParse(history.Data.GetValueOrDefault("indexerFlags"), true, out IndexerFlags indexerFlags);

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
                Languages = history.Languages,
                IndexerFlags = indexerFlags
            };

            return ParseCustomFormat(input);
        }

        public List<CustomFormat> ParseCustomFormat(LocalMovie localMovie)
        {
            var movieInfo = new ParsedMovieInfo
            {
                MovieTitles = new List<string>() { localMovie.Movie.Title },
                SimpleReleaseTitle = localMovie.SceneName.IsNotNullOrWhiteSpace() ? localMovie.SceneName.SimplifyReleaseTitle() : Path.GetFileName(localMovie.Path).SimplifyReleaseTitle(),
                ReleaseTitle = localMovie.SceneName,
                Quality = localMovie.Quality,
                Edition = localMovie.Edition,
                Languages = localMovie.Languages,
                ReleaseGroup = localMovie.ReleaseGroup
            };

            var input = new CustomFormatInput
            {
                MovieInfo = movieInfo,
                Movie = localMovie.Movie,
                Size = localMovie.Size,
                Languages = localMovie.Languages,
                IndexerFlags = localMovie.IndexerFlags,
                Filename = Path.GetFileName(localMovie.Path)
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

            return matches.OrderBy(x => x.Name).ToList();
        }

        private List<CustomFormat> ParseCustomFormat(MovieFile movieFile, Movie movie, List<CustomFormat> allCustomFormats)
        {
            var releaseTitle = string.Empty;

            if (movieFile.SceneName.IsNotNullOrWhiteSpace())
            {
                _logger.Trace("Using scene name for release title: {0}", movieFile.SceneName);
                releaseTitle = movieFile.SceneName;
            }
            else if (movieFile.OriginalFilePath.IsNotNullOrWhiteSpace())
            {
                _logger.Trace("Using original file path for release title: {0}", Path.GetFileName(movieFile.OriginalFilePath));
                releaseTitle = Path.GetFileName(movieFile.OriginalFilePath);
            }
            else if (movieFile.RelativePath.IsNotNullOrWhiteSpace())
            {
                _logger.Trace("Using relative path for release title: {0}", Path.GetFileName(movieFile.RelativePath));
                releaseTitle = Path.GetFileName(movieFile.RelativePath);
            }

            var movieInfo = new ParsedMovieInfo
            {
                MovieTitles = new List<string>() { movie.Title },
                SimpleReleaseTitle = releaseTitle.SimplifyReleaseTitle(),
                Quality = movieFile.Quality,
                Languages = movieFile.Languages,
                ReleaseGroup = movieFile.ReleaseGroup,
                Edition = movieFile.Edition
            };

            var input = new CustomFormatInput
            {
                MovieInfo = movieInfo,
                Movie = movie,
                Size = movieFile.Size,
                Languages = movieFile.Languages,
                IndexerFlags = movieFile.IndexerFlags,
                Filename = Path.GetFileName(movieFile.RelativePath)
            };

            return ParseCustomFormat(input, allCustomFormats);
        }
    }
}
