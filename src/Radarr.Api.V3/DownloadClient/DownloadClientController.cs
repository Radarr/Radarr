using NzbDrone.Core.Download;
using Radarr.Http;

namespace Radarr.Api.V3.DownloadClient
{
    [V3ApiController]
    public class DownloadClientController : ProviderControllerBase<DownloadClientResource, DownloadClientBulkResource, IDownloadClient, DownloadClientDefinition>
    {
        public static readonly DownloadClientResourceMapper ResourceMapper = new ();
        public static readonly DownloadClientBulkResourceMapper BulkResourceMapper = new ();

        public DownloadClientController(IDownloadClientFactory downloadClientFactory)
            : base(downloadClientFactory, "downloadclient", ResourceMapper, BulkResourceMapper)
        {
        }
    }
}
