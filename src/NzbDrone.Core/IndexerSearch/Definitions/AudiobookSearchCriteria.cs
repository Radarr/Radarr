namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public class AudiobookSearchCriteria : SearchCriteriaBase
    {
        public string Author { get; set; }
        public string Title { get; set; }
        public string Narrator { get; set; }
        public string ASIN { get; set; }
        public string ISBN { get; set; }
        public int? Year { get; set; }
        public bool? Abridged { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(Author) && !string.IsNullOrWhiteSpace(Title))
            {
                var result = $"[{Author} - {Title}";
                if (!string.IsNullOrWhiteSpace(Narrator))
                {
                    result += $" (narrated by {Narrator})";
                }

                return result + "]";
            }

            if (!string.IsNullOrWhiteSpace(ASIN))
            {
                return $"[ASIN: {ASIN}]";
            }

            return $"[{Title ?? Author ?? "Unknown"}]";
        }
    }
}
