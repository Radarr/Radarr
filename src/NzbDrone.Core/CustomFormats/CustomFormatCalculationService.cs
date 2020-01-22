using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Blacklisting;
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
        List<CustomFormat> ParseCustomFormat(Blacklist blacklist);
        List<CustomFormat> ParseCustomFormat(History.History history);
        List<CustomFormatMatchResult> MatchFormatTags(ParsedMovieInfo movieInfo);
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

        public List<CustomFormat> ParseCustomFormat(ParsedMovieInfo movieInfo)
        {
            return MatchFormatTags(movieInfo)
                .Where(m => m.GoodMatch)
                .Select(r => r.CustomFormat)
                .ToList();
        }

        public List<CustomFormat> ParseCustomFormat(MovieFile movieFile)
        {
            return MatchFormatTags(movieFile)
                .Where(m => m.GoodMatch)
                .Select(r => r.CustomFormat)
                .ToList();
        }

        public List<CustomFormat> ParseCustomFormat(Blacklist blacklist)
        {
            return MatchFormatTags(blacklist)
                .Where(m => m.GoodMatch)
                .Select(r => r.CustomFormat)
                .ToList();
        }

        public List<CustomFormat> ParseCustomFormat(History.History history)
        {
            return MatchFormatTags(history)
                .Where(m => m.GoodMatch)
                .Select(r => r.CustomFormat)
                .ToList();
        }

        public List<CustomFormatMatchResult> MatchFormatTags(ParsedMovieInfo movieInfo)
        {
            var formats = _formatService.All();

            var matches = new List<CustomFormatMatchResult>();

            foreach (var customFormat in formats)
            {
                var tagTypeMatches = customFormat.FormatTags
                    .GroupBy(t => t.TagType)
                    .Select(g => new FormatTagMatchesGroup
                    {
                        Type = g.Key,
                        Matches = g.ToDictionary(t => t, t => t.DoesItMatch(movieInfo))
                    })
                    .ToList();

                matches.Add(new CustomFormatMatchResult
                {
                    CustomFormat = customFormat,
                    GroupMatches = tagTypeMatches
                });
            }

            return matches;
        }

        private List<CustomFormatMatchResult> MatchFormatTags(MovieFile file)
        {
            var info = new ParsedMovieInfo
            {
                MovieTitle = file.Movie.Title,
                SimpleReleaseTitle = file.GetSceneOrFileName().SimplifyReleaseTitle(),
                Quality = file.Quality,
                Languages = file.Languages,
                ReleaseGroup = file.ReleaseGroup,
                Edition = file.Edition,
                Year = file.Movie.Year,
                ImdbId = file.Movie.ImdbId,
                ExtraInfo = new Dictionary<string, object>
                {
                    { "IndexerFlags", file.IndexerFlags },
                    { "Size", file.Size },
                    { "Filename", System.IO.Path.GetFileName(file.RelativePath) }
                }
            };

            return MatchFormatTags(info);
        }

        private List<CustomFormatMatchResult> MatchFormatTags(Blacklist blacklist)
        {
            var parsed = _parsingService.ParseMovieInfo(blacklist.SourceTitle, null);

            var info = new ParsedMovieInfo
            {
                MovieTitle = blacklist.Movie.Title,
                SimpleReleaseTitle = parsed?.SimpleReleaseTitle ?? blacklist.SourceTitle.SimplifyReleaseTitle(),
                Quality = blacklist.Quality,
                Languages = blacklist.Languages,
                ReleaseGroup = parsed?.ReleaseGroup,
                Edition = parsed?.Edition,
                Year = blacklist.Movie.Year,
                ImdbId = blacklist.Movie.ImdbId,
                ExtraInfo = new Dictionary<string, object>
                {
                    { "IndexerFlags", blacklist.IndexerFlags },
                    { "Size", blacklist.Size }
                }
            };

            return MatchFormatTags(info);
        }

        private List<CustomFormatMatchResult> MatchFormatTags(History.History history)
        {
            var movie = _movieService.GetMovie(history.MovieId);
            var parsed = _parsingService.ParseMovieInfo(history.SourceTitle, null);

            Enum.TryParse(history.Data.GetValueOrDefault("indexerFlags"), true, out IndexerFlags flags);
            int.TryParse(history.Data.GetValueOrDefault("size"), out var size);

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

            return MatchFormatTags(info);
        }
    }
}
