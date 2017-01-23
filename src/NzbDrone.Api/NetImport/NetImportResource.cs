using NzbDrone.Core.NetImport;

namespace NzbDrone.Api.NetImport
{
    public class NetImportResource : ProviderResource
    {
        public bool Enabled { get; set; }
        public bool EnableAuto { get; set; }
        public bool ShouldMonitor { get; set; }
        public string RootFolderPath { get; set; }
        public int ProfileId { get; set; }
    }
}