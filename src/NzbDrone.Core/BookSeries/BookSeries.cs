using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.BookSeries
{
    public class BookSeries : ModelBase
    {
        public string Title { get; set; }
        public string SortTitle { get; set; }
        public string Description { get; set; }
        public string ForeignSeriesId { get; set; }
        public int? AuthorId { get; set; }
        public bool Monitored { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
