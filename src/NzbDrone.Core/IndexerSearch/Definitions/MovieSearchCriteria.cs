namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public class MovieSearchCriteria : SearchCriteriaBase
    {

        public override string ToString()
        {
            return string.Format("[{0}]", Movie.Title);
        }
    }
}
