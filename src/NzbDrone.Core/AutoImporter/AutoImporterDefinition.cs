using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.AutoImporter
{
    public class AutoImporterDefinition : ProviderDefinition
    {
        public bool Enabled { get; set; }
        public string Link { get; set; }
        //public DownloadProtocol Protocol { get; set; }
        //public bool SupportsRss { get; set; }
        //public bool SupportsSearch { get; set; }

        public override bool Enable => Enabled;

        // public IndexerStatus Status { get; set; }
    }
}
