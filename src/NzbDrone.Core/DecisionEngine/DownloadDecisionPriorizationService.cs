using System.Linq;
using System.Collections.Generic;
using NzbDrone.Core.Profiles.Delay;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.DecisionEngine
{
    public interface IPrioritizeDownloadDecision
    {
        List<DownloadDecision> PrioritizeDecisions(List<DownloadDecision> decisions);
        List<DownloadDecision> PrioritizeDecisionsForMovies(List<DownloadDecision> decisions);
    }

    public class DownloadDecisionPriorizationService : IPrioritizeDownloadDecision
    {
        private readonly IDelayProfileService _delayProfileService;
        private readonly IConfigService _configService;

        public DownloadDecisionPriorizationService(IDelayProfileService delayProfileService, IConfigService configService)
        {
            _delayProfileService = delayProfileService;
            _configService = configService;
        }

        public List<DownloadDecision> PrioritizeDecisions(List<DownloadDecision> decisions)
        {
            return decisions.Where(c => c.RemoteEpisode.Series != null)
                            .GroupBy(c => c.RemoteEpisode.Series.Id, (seriesId, downloadDecisions) =>
                                {
                                    return downloadDecisions.OrderByDescending(decision => decision, new DownloadDecisionComparer(_delayProfileService, _configService));
                                })
                            .SelectMany(c => c)
                            .Union(decisions.Where(c => c.RemoteEpisode.Series == null))
                            .ToList();
        }

        public List<DownloadDecision> PrioritizeDecisionsForMovies(List<DownloadDecision> decisions)
        {
            return decisions.Where(c => c.RemoteMovie.MappingResult == MappingResultType.Success || c.RemoteMovie.MappingResult == MappingResultType.SuccessLenientMapping)
                            .GroupBy(c => c.RemoteMovie.Movie.Id, (movieId, downloadDecisions) =>
                            {
                                return downloadDecisions.OrderByDescending(decision => decision, new DownloadDecisionComparer(_delayProfileService, _configService));
                            })
                            .SelectMany(c => c)
                            .Union(decisions.Where(c => c.RemoteMovie.MappingResult != MappingResultType.Success || c.RemoteMovie.MappingResult != MappingResultType.SuccessLenientMapping))
                            .ToList();
        }
    }
}
