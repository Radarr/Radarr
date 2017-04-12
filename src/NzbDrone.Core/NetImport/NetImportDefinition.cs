using Marr.Data;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.NetImport
{
    public class NetImportDefinition : ProviderDefinition
    {
        public bool Enabled { get; set; }
        public bool EnableAuto { get; set; }
		public bool ShouldClean { get; set; }
        public bool ShouldMonitor { get; set; }
        public MovieStatusType MinimumAvailability { get; set; }
        public int ProfileId { get; set; }
        public LazyLoaded<Profile> Profile { get; set; }
        public string RootFolderPath { get; set; }
        public override bool Enable => Enabled;
    }
}
