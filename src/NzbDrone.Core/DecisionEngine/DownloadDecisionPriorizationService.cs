using System.Linq;
using System.Collections.Generic;
using NzbDrone.Core.Profiles.Delay;

namespace NzbDrone.Core.DecisionEngine
{
    public interface IPrioritizeDownloadDecision
    {
        List<DownloadDecision> PrioritizeDecisions(List<DownloadDecision> decisions);
    }

    public class DownloadDecisionPriorizationService : IPrioritizeDownloadDecision
    {
        private readonly IDelayProfileService _delayProfileService;

        public DownloadDecisionPriorizationService(IDelayProfileService delayProfileService)
        {
            _delayProfileService = delayProfileService;
        }

        public List<DownloadDecision> PrioritizeDecisions(List<DownloadDecision> decisions)
        {
            return decisions.Where(c => c.RemoteAlbum.Artist != null)
                            .GroupBy(c => c.RemoteAlbum.Artist.Id, (artistId, downloadDecisions) =>
                                {
                                    return downloadDecisions.OrderByDescending(decision => decision, new DownloadDecisionComparer(_delayProfileService));
                                })
                            .SelectMany(c => c)
                            .Union(decisions.Where(c => c.RemoteAlbum.Artist == null))
                            .ToList();
        }
    }
}
