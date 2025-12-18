namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public class BookSearchCriteria : SearchCriteriaBase
    {
        public string Author { get; set; }
        public string Title { get; set; }
        public string ISBN { get; set; }
        public string Publisher { get; set; }
        public int? Year { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(Author) && !string.IsNullOrWhiteSpace(Title))
            {
                return $"[{Author} - {Title}]";
            }

            if (!string.IsNullOrWhiteSpace(ISBN))
            {
                return $"[ISBN: {ISBN}]";
            }

            return $"[{Title ?? Author ?? "Unknown"}]";
        }
    }
}
