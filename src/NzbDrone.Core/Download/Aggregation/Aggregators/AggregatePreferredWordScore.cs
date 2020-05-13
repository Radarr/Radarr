using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Releases;

namespace NzbDrone.Core.Download.Aggregation.Aggregators
{
    public class AggregatePreferredWordScore : IAggregateRemoteAlbum
    {
        private readonly IPreferredWordService _preferredWordServiceCalculator;

        public AggregatePreferredWordScore(IPreferredWordService preferredWordServiceCalculator)
        {
            _preferredWordServiceCalculator = preferredWordServiceCalculator;
        }

        public RemoteBook Aggregate(RemoteBook remoteAlbum)
        {
            remoteAlbum.PreferredWordScore = _preferredWordServiceCalculator.Calculate(remoteAlbum.Author, remoteAlbum.Release.Title);

            return remoteAlbum;
        }
    }
}
