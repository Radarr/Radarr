using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.NetImport
{
    public class NetImportDefinition : ProviderDefinition
    {
        public bool Enabled { get; set; }
        public override bool Enable => Enabled;
    }
}
