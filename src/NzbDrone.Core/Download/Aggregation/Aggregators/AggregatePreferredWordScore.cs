using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Releases;

namespace NzbDrone.Core.Download.Aggregation.Aggregators
{
    public class AggregatePreferredWordScore : IAggregateRemoteBook
    {
        private readonly IPreferredWordService _preferredWordServiceCalculator;

        public AggregatePreferredWordScore(IPreferredWordService preferredWordServiceCalculator)
        {
            _preferredWordServiceCalculator = preferredWordServiceCalculator;
        }

        public RemoteBook Aggregate(RemoteBook remoteBook)
        {
            remoteBook.PreferredWordScore = _preferredWordServiceCalculator.Calculate(remoteAlbum.Author, remoteBook.Release.Title, remoteBook.Release.IndexerId);

            return remoteBook;
        }
    }
}
