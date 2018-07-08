using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.ImportLists
{
    public class ImportListDefinition : ProviderDefinition
    {
        public bool EnableAutomaticAdd { get; set; }
        public bool ShouldMonitor { get; set; }
        public int ProfileId { get; set; }
        public int LanguageProfileId { get; set; }
        public int MetadataProfileId { get; set; }
        public string RootFolderPath { get; set; }

        public override bool Enable => EnableAutomaticAdd;

        public ImportListStatus Status { get; set; }
    }
}
