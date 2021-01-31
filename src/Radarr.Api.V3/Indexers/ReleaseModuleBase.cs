using System.Collections.Generic;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Profiles;
using Radarr.Http;

namespace Radarr.Api.V3.Indexers
{
    public abstract class ReleaseModuleBase : RadarrRestModule<ReleaseResource>
    {
        private readonly Profile _qualityProfie;

        public ReleaseModuleBase(IProfileService qualityProfileService)
        {
            _qualityProfie = qualityProfileService.GetDefaultProfile(string.Empty);
        }

        protected virtual List<ReleaseResource> MapDecisions(IEnumerable<DownloadDecision> decisions)
        {
            var result = new List<ReleaseResource>();

            foreach (var downloadDecision in decisions)
            {
                var release = MapDecision(downloadDecision, result.Count);

                result.Add(release);
            }

            return result;
        }

        protected virtual ReleaseResource MapDecision(DownloadDecision decision, int initialWeight)
        {
            var release = decision.ToResource();

            release.ReleaseWeight = initialWeight;

            release.QualityWeight = _qualityProfie.GetIndex(release.Quality.Quality).Index * 100;

            release.QualityWeight += release.Quality.Revision.Real * 10;
            release.QualityWeight += release.Quality.Revision.Version;

            return release;
        }
    }
}
