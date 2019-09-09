using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download.Aggregation.Aggregators
{
    public interface IAggregateRemoteAlbum
    {
        RemoteAlbum Aggregate(RemoteAlbum remoteAlbum);
    }
}
