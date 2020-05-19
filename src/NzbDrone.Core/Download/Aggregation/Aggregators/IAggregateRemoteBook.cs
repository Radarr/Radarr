using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download.Aggregation.Aggregators
{
    public interface IAggregateRemoteBook
    {
        RemoteBook Aggregate(RemoteBook remoteBook);
    }
}
