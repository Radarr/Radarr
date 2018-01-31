using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class FullSeasonSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public FullSeasonSpecification(Logger logger)
        {
            _logger = logger;
        }

        public Decision IsSatisfiedBy(LocalEpisode localEpisode)
        {
            if (localEpisode.ParsedEpisodeInfo.FullSeason)
            {
                _logger.Debug("Single episode file detected as containing all episodes in the season"); //Not needed for Movies mwhahahahah
                return Decision.Reject("Single episode file contains all episodes in seasons");
            }

            return Decision.Accept();
        }

        public Decision IsSatisfiedBy(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            return Decision.Accept();
        }
    }
}
