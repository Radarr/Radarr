using System;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaTypes;

namespace NzbDrone.Core.Books
{
    public class Book : ModelBase
    {
        public Book()
        {
            Tags = new HashSet<int>();
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

        public MediaType MediaType { get; set; }
        public bool Monitored { get; set; }
        public int QualityProfileId { get; set; }
        public string Path { get; set; }
        public string RootFolderPath { get; set; }
        public DateTime Added { get; set; }
        public HashSet<int> Tags { get; set; }
        public DateTime? LastSearchTime { get; set; }

        public int? AuthorId { get; set; }
        public int? SeriesId { get; set; }
        public int? SeriesPosition { get; set; }

        public override string ToString()
        {
            return $"{Title} ({ReleaseDate?.Year})";
        }
    }
}
