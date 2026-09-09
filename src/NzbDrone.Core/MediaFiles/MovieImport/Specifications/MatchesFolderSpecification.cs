using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport.Specifications
{
    public class MatchesFolderSpecification : IImportDecisionEngineSpecification
    {
        private readonly IParsingService _parsingService;
        private readonly Logger _logger;

        public MatchesFolderSpecification(IParsingService parsingService, Logger logger)
        {
            _parsingService = parsingService;
            _logger = logger;
        }

        public ImportSpecDecision IsSatisfiedBy(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            if (localMovie.ExistingFile)
            {
                return ImportSpecDecision.Accept();
            }

            if (localMovie.FolderMovieInfo == null)
            {
                return ImportSpecDecision.Accept();
            }

            var fileInfo = localMovie.FileMovieInfo;

            if (fileInfo == null || fileInfo.PrimaryMovieTitle.IsNullOrWhiteSpace())
            {
                return ImportSpecDecision.Accept();
            }

            var fileMovie = _parsingService.Map(fileInfo, fileInfo.ImdbId, fileInfo.TmdbId)?.Movie;

            if (fileMovie == null || fileMovie.Id == localMovie.Movie.Id)
            {
                return ImportSpecDecision.Accept();
            }

            _logger.Debug("File {0} mapped to movie {1}, which does not match the movie being imported: {2}", localMovie.Path, fileMovie, localMovie.Movie);

            return ImportSpecDecision.Reject(ImportRejectionReason.MovieDoesNotMatch, "Movie in file ({0}) does not match the movie being imported: {1}", fileMovie.Title, localMovie.Movie.Title);
        }
    }
}
