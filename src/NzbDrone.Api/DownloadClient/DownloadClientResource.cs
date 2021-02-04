using NzbDrone.Core.Indexers;

namespace NzbDrone.Api.DownloadClient
{
    public class DownloadClientResource : ProviderResource<DownloadClientResource>
    {
        public bool Enable { get; set; }
        public DownloadProtocol Protocol { get; set; }
        public int Priority { get; set; }
    }
}
