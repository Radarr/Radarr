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
                                                .QualityProfile.Value
                                                .Items.FindIndex(v => v.Quality == release.Quality.Quality) * 100;

                release.LanguageWeight = decision.RemoteAlbum
                                 .Artist
                                 .LanguageProfile.Value
                                 .Languages.FindIndex(v => v.Language == release.Language) * 100;

            }

            release.QualityWeight += release.Quality.Revision.Real * 10;
            release.QualityWeight += release.Quality.Revision.Version;

            return release;
        }
    }
}
