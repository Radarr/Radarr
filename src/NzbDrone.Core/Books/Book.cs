using System;
using NzbDrone.Core.MediaItems;
using NzbDrone.Core.MediaTypes;

namespace NzbDrone.Core.Books
{
    public class Book : MediaItem
    {
        public Book()
        {
            MediaType = MediaType.Book;
        }

        public string Title { get; set; }
        public string SortTitle { get; set; }
        public string Description { get; set; }
        public string ForeignBookId { get; set; }
        public string Isbn { get; set; }
        public string Isbn13 { get; set; }
        public string Asin { get; set; }
        public int? PageCount { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string Publisher { get; set; }
        public string Language { get; set; }

        public int? SeriesPosition { get; set; }

        public override string GetTitle() => Title;
        public override int GetYear() => ReleaseDate?.Year ?? 0;

        public override string ToString()
        {
            return $"{Title} ({ReleaseDate?.Year})";
        }
    }
}
