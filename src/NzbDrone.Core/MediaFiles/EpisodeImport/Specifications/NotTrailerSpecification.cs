using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class NotTrailerSpecification : IImportDecisionEngineSpecification
    {
        private readonly IDetectTrailer _detectTrailer;
        private readonly Logger _logger;

        public NotTrailerSpecification(IDetectTrailer detectTrailer,
                                      Logger logger)
        {
            _detectTrailer = detectTrailer;
            _logger = logger;
        }

        public Decision IsSatisfiedBy(LocalEpisode localEpisode)
        {
            return Decision.Accept();
        }

        public Decision IsSatisfiedBy(LocalMovie localMovie)
        {
            var trailer = _detectTrailer.IsTrailer(localMovie.Movie,
                                                localMovie.Quality,
                                                localMovie.Path,
                                                localMovie.Size,
                                                false);

            if (trailer)
            {
                return Decision.Reject("Trailer");
            }

            return Decision.Accept();
        }
    }
}
