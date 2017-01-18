using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.NetImport
{
    public class NetImportDefinition : ProviderDefinition
    {
        public string Link { get; set; }
        public int ProfileId { get; set; }
        public bool Enabled { get; set; }
        public override bool Enable => Enabled;
    }
}
