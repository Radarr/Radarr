using System.Collections.Generic;
using NzbDrone.Core.DecisionEngine;
using Lidarr.Http;

namespace Lidarr.Api.V1.Indexers
{
    public abstract class ReleaseModuleBase : LidarrRestModule<ReleaseResource>
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

            if (decision.RemoteAlbum.Artist != null)
            {
                release.QualityWeight = decision.RemoteAlbum
                                                .Artist
                                                .QualityProfile.Value.GetIndex(release.Quality.Quality).Index * 100;
            }

            release.QualityWeight += release.Quality.Revision.Real * 10;
            release.QualityWeight += release.Quality.Revision.Version;

            return release;
        }
    }
}
