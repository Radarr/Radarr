using System;
using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource.Book
{
    public interface IProvideBookInfo : IProvideMediaInfo<BookMetadata>, ISearchableMediaProvider<BookMetadata>
    {
        BookMetadata GetByIsbn(string isbn);
        BookMetadata GetByIsbn13(string isbn13);
        BookMetadata GetByAsin(string asin);
        List<BookMetadata> GetByAuthor(string authorName);
    }

    public class BookMetadata
    {
        public string ForeignBookId { get; set; }
        public string Title { get; set; }
        public string SortTitle { get; set; }
        public string Description { get; set; }
        public string Isbn { get; set; }
        public string Isbn13 { get; set; }
        public string Asin { get; set; }
        public int? PageCount { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string Publisher { get; set; }
        public string Language { get; set; }
        public List<string> Authors { get; set; }
        public List<string> Genres { get; set; }
        public string CoverUrl { get; set; }
        public double? Rating { get; set; }
        public int? RatingsCount { get; set; }
    }
}
