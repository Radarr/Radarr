using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download.Aggregation.Aggregators
{
    public interface IAggregateRemoteMovie
    {
        RemoteMovie Aggregate(RemoteMovie remoteMovie);
    }
}
