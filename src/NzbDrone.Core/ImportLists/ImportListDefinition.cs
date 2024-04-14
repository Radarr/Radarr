using System;
using Equ;
using NzbDrone.Core.Movies;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.ImportLists
{
    public class ImportListDefinition : ProviderDefinition, IEquatable<ImportListDefinition>
    {
        private static readonly MemberwiseEqualityComparer<ImportListDefinition> Comparer = MemberwiseEqualityComparer<ImportListDefinition>.ByProperties;

        public bool Enabled { get; set; }
        public bool EnableAuto { get; set; }
        public MonitorTypes Monitor { get; set; }
        public MovieStatusType MinimumAvailability { get; set; }
        public int QualityProfileId { get; set; }
        public string RootFolderPath { get; set; }
        public bool SearchOnAdd { get; set; }

        [MemberwiseEqualityIgnore]
        public override bool Enable => Enabled;

        [MemberwiseEqualityIgnore]
        public ImportListType ListType { get; set; }

        [MemberwiseEqualityIgnore]
        public TimeSpan MinRefreshInterval { get; set; }

        public bool Equals(ImportListDefinition other)
        {
            return Comparer.Equals(this, other);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ImportListDefinition);
        }

        public override int GetHashCode()
        {
            return Comparer.GetHashCode(this);
        }
    }
}
