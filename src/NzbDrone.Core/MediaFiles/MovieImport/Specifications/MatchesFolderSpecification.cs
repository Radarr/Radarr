using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport.Specifications
{
    public class MatchesFolderSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public MatchesFolderSpecification(Logger logger)
        {
            _logger = logger;
        }

        public ImportSpecDecision IsSatisfiedBy(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            if (localMovie.ExistingFile)
            {
                return ImportSpecDecision.Accept();
            }

            var fileInfo = localMovie.FileMovieInfo;

            if (fileInfo == null || fileInfo.PrimaryMovieTitle.IsNullOrWhiteSpace())
            {
                return ImportSpecDecision.Accept();
            }

            var movie = localMovie.Movie;
            var movieMetadata = movie.MovieMetadata.Value;

            if (fileInfo.TmdbId > 0 && movie.TmdbId > 0)
            {
                if (fileInfo.TmdbId != movie.TmdbId)
                {
                    _logger.Debug("TMDB ID {0} in file {1} does not match movie being imported: {2}", fileInfo.TmdbId, localMovie.Path, movie);

                    return ImportSpecDecision.Reject(ImportRejectionReason.MovieDoesNotMatch, "TMDB ID {0} in file does not match movie: {1}", fileInfo.TmdbId, movie.Title);
                }

                return ImportSpecDecision.Accept();
            }

            if (fileInfo.ImdbId.IsNotNullOrWhiteSpace() && movie.ImdbId.IsNotNullOrWhiteSpace())
            {
                if (fileInfo.ImdbId != movie.ImdbId)
                {
                    _logger.Debug("IMDb ID {0} in file {1} does not match movie being imported: {2}", fileInfo.ImdbId, localMovie.Path, movie);

                    return ImportSpecDecision.Reject(ImportRejectionReason.MovieDoesNotMatch, "IMDb ID {0} in file does not match movie: {1}", fileInfo.ImdbId, movie.Title);
                }

                return ImportSpecDecision.Accept();
            }

            if (fileInfo.Year <= 1800 || movieMetadata.Year == 0)
            {
                return ImportSpecDecision.Accept();
            }

            if (fileInfo.Year == movieMetadata.Year || fileInfo.Year == movieMetadata.SecondaryYear)
            {
                return ImportSpecDecision.Accept();
            }

            // The parsed year may be part of the title itself (e.g. Blade Runner 2049)
            var movieTitles = new List<string> { movieMetadata.CleanTitle };
            movieTitles.AddIfNotNull(movieMetadata.CleanOriginalTitle);
            movieTitles.AddRange(movieMetadata.AlternativeTitles.Select(t => t.CleanTitle));
            movieTitles.AddRange(movieMetadata.Translations.Select(t => t.CleanTitle));

            if (fileInfo.MovieTitles.Any(t => movieTitles.Contains($"{t} {fileInfo.Year}".CleanMovieTitle())))
            {
                return ImportSpecDecision.Accept();
            }

            _logger.Debug("Year {0} in file {1} does not match movie being imported: {2}", fileInfo.Year, localMovie.Path, movie);

            return ImportSpecDecision.Reject(ImportRejectionReason.MovieDoesNotMatch, "Year {0} in file does not match movie: {1} ({2})", fileInfo.Year, movie.Title, movieMetadata.Year);
        }
    }
}
