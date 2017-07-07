using System;

namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public class AlbumSearchCriteria : SearchCriteriaBase
    {
        public override string ToString()
        {
            return $"[{Album.Title}]";
        }
    }
}
