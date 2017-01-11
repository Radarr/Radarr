using System.Collections.Generic;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.AutoImport
{
    public class AutoImportDefinition : ProviderDefinition
    {
        public AutoImportDefinition()
        {
            Tags = new HashSet<int>();
        }

        public bool Enabled { get; set; }
        public HashSet<int> Tags { get; set; }

        public override bool Enable => Enabled;
    }
}
