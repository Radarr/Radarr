using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.AutoImporter
{
    public class AutoImporterDefinition : ProviderDefinition
    {
        public bool Enabled { get; set; }
        public string Link { get; set; }
        public override bool Enable => Enabled;
    }
}
