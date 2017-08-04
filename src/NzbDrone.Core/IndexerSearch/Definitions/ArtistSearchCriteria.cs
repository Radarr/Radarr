using System;

namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public class ArtistSearchCriteria : SearchCriteriaBase
    {
        public override string ToString()
        {
            return $"[{Artist.Name}]";
        }
    }
}
