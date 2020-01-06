using System.Collections.Generic;
using NzbDrone.Core.DecisionEngine;
using Radarr.Http;

namespace NzbDrone.Api.Indexers
{
    public abstract class ReleaseModuleBase : RadarrRestModule<ReleaseResource>
    {
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

            if (decision.RemoteMovie.Movie != null)
            {
                release.QualityWeight = decision.RemoteMovie.Movie
                                                              .Profile
                                                              .Items.FindIndex(v => v.Quality == release.Quality.Quality) * 100;
            }

            release.QualityWeight += release.Quality.Revision.Real * 10;
            release.QualityWeight += release.Quality.Revision.Version;

            return release;
        }
    }
}
