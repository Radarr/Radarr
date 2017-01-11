using System.Collections.Generic;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.AutoImporter
{
    public interface IAutoImporter : IProvider
    {
        string Link { get; }
        bool Enabled { get; }
        
        IList<ReleaseInfo> Fetch();
    }
}