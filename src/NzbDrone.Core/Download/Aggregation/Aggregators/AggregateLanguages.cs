using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download.Aggregation.Aggregators
{
    public class AggregateLanguages : IAggregateRemoteMovie
    {
        private readonly Logger _logger;

        public AggregateLanguages(Logger logger)
        {
            _logger = logger;
        }

        public RemoteMovie Aggregate(RemoteMovie remoteMovie)
        {
            var parsedMovieInfo = remoteMovie.ParsedMovieInfo;
            var languages = parsedMovieInfo.Languages;
            var movie = remoteMovie.Movie;
            var releaseTokens = parsedMovieInfo.SimpleReleaseTitle ?? parsedMovieInfo.ReleaseTitle;
            var normalizedReleaseTokens = Parser.Parser.NormalizeEpisodeTitle(releaseTokens);
            var languagesToRemove = new List<Language>();

            if (movie == null)
            {
                _logger.Debug("Unable to aggregate languages, using parsed values: {0}", string.Join(", ", languages.ToList()));

                remoteMovie.Languages = languages;

                return remoteMovie;
            }

            var episodeTitleLanguage = LanguageParser.ParseLanguages(movie.Title);

            if (!episodeTitleLanguage.Contains(Language.Unknown))
            {
                var normalizedEpisodeTitle = Parser.Parser.NormalizeEpisodeTitle(movie.Title);
                var episodeTitleIndex = normalizedReleaseTokens.IndexOf(normalizedEpisodeTitle, StringComparison.CurrentCultureIgnoreCase);

                if (episodeTitleIndex >= 0)
                {
                    releaseTokens = releaseTokens.Remove(episodeTitleIndex, normalizedEpisodeTitle.Length);
                    languagesToRemove.AddRange(episodeTitleLanguage);
                }
            }

            // Remove any languages still in the title that would normally be removed
            languagesToRemove = languagesToRemove.Except(LanguageParser.ParseLanguages(releaseTokens)).ToList();

            // Remove all languages that aren't part of the updated releaseTokens
            languages = languages.Except(languagesToRemove).ToList();

            // Use series language as fallback if we couldn't parse a language
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
