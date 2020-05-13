using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public class BookSearchCriteria : SearchCriteriaBase
    {
        public string BookTitle { get; set; }
        public int BookYear { get; set; }
        public string Disambiguation { get; set; }

        public string AlbumQuery => GetQueryTitle($"{BookTitle}{(Disambiguation.IsNullOrWhiteSpace() ? string.Empty : $"+{Disambiguation}")}");

        public override string ToString()
        {
            return $"[{Author.Name} - {BookTitle}{(Disambiguation.IsNullOrWhiteSpace() ? string.Empty : $" ({Disambiguation})")} ({BookYear})]";
        }
    }
}
