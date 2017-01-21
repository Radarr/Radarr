using NzbDrone.Core.NetImport;

namespace NzbDrone.Api.NetImport
{
    public class NetImportResource : ProviderResource
    {
        public bool Enabled { get; set; }
        public bool EnableSearch { get; set; }
    }
}