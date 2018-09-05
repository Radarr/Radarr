using System;

namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public class AlbumSearchCriteria : SearchCriteriaBase
    {

        public string AlbumTitle { get; set; }
        public int AlbumYear { get; set; }

        public string AlbumQuery => GetQueryTitle(AlbumTitle);

        public override string ToString()
        {
            return string.Format("[{0} - {1} ({2})]", Artist.Name, AlbumTitle, AlbumYear);
        }
    }
}
