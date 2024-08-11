using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download.Aggregation.Aggregators
{
    public class AggregateLanguages : IAggregateRemoteMovie
    {
        private readonly IIndexerFactory _indexerFactory;
        private readonly Logger _logger;

        public AggregateLanguages(IIndexerFactory indexerFactory,
                                  Logger logger)
        {
            _indexerFactory = indexerFactory;
            _logger = logger;
        }

        public RemoteMovie Aggregate(RemoteMovie remoteMovie)
        {
            var parsedMovieInfo = remoteMovie.ParsedMovieInfo;
            var releaseInfo = remoteMovie.Release;
            var languages = parsedMovieInfo.Languages;
            var movie = remoteMovie.Movie;
            var releaseTokens = parsedMovieInfo.SimpleReleaseTitle ?? parsedMovieInfo.ReleaseTitle;
            var normalizedReleaseTokens = Parser.Parser.NormalizeEpisodeTitle(releaseTokens);
            var languagesToRemove = new List<Language>();

            if (movie == null)
            {
                _logger.Debug("Unable to aggregate languages, using parsed values: {0}", string.Join(", ", languages.ToList()));

                remoteMovie.Languages = releaseInfo != null && releaseInfo.Languages.Any() ? releaseInfo.Languages : languages;

                return remoteMovie;
            }

            if (releaseInfo != null && releaseInfo.Languages.Any())
            {
                _logger.Debug("Languages provided by indexer, using release values: {0}", string.Join(", ", releaseInfo.Languages));

                // Use languages from release (given by indexer or user) if available
                languages = releaseInfo.Languages;
            }
            else
            {
                var movieTitleLanguage = LanguageParser.ParseLanguages(movie.Title);

                if (!movieTitleLanguage.Contains(Language.Unknown))
                {
                    var normalizedEpisodeTitle = Parser.Parser.NormalizeEpisodeTitle(movie.Title);
                    var movieTitleIndex = normalizedReleaseTokens.IndexOf(normalizedEpisodeTitle, StringComparison.CurrentCultureIgnoreCase);

                    if (movieTitleIndex >= 0)
                    {
                        releaseTokens = releaseTokens.Remove(movieTitleIndex, normalizedEpisodeTitle.Length);
                        languagesToRemove.AddRange(movieTitleLanguage);
                    }
                }

                // Remove any languages still in the title that would normally be removed
                languagesToRemove = languagesToRemove.Except(LanguageParser.ParseLanguages(releaseTokens)).ToList();

                // Remove all languages that aren't part of the updated releaseTokens
                languages = languages.Except(languagesToRemove).ToList();
            }

            if ((languages.Count == 0 || (languages.Count == 1 && languages.First() == Language.Unknown)) && releaseInfo?.Title?.IsNotNullOrWhiteSpace() == true)
            {
                IndexerDefinition indexer = null;

                if (releaseInfo is { IndexerId: > 0 })
                {
                    indexer = _indexerFactory.Get(releaseInfo.IndexerId);
                }
                else if (releaseInfo.Indexer?.IsNotNullOrWhiteSpace() == true)
                {
                    indexer = _indexerFactory.FindByName(releaseInfo.Indexer);
                }

                if (indexer?.Settings is IIndexerSettings settings && settings.MultiLanguages.Any() && Parser.Parser.HasMultipleLanguages(releaseInfo.Title))
                {
                    // Use indexer setting for Multi-languages
                    languages = settings.MultiLanguages.Select(i => (Language)i).ToList();
                }
            }

            // Use movie language as fallback if we couldn't parse a language
            if (languages.Count == 0 || (languages.Count == 1 && languages.First() == Language.Unknown))
            {
                languages = new List<Language> { movie.MovieMetadata.Value.OriginalLanguage };
                _logger.Debug("Language couldn't be parsed from release, fallback to movie original language: {0}", movie.MovieMetadata.Value.OriginalLanguage.Name);
            }

            if (languages.Contains(Language.Original))
            {
                languages.Remove(Language.Original);

                if (!languages.Contains(movie.MovieMetadata.Value.OriginalLanguage))
                {
                    languages.Add(movie.MovieMetadata.Value.OriginalLanguage);
                }
                else
                {
                    languages.Add(Language.Unknown);
                }
            }

            _logger.Debug("Selected languages: {0}", string.Join(", ", languages.ToList()));

            remoteMovie.Languages = languages;

            return remoteMovie;
        }
    }
}
