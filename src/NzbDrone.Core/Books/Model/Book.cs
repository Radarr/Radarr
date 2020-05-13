using System;
using System.Collections.Generic;
using System.Linq;
using Equ;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Books
{
    public class Book : Entity<Book>
    {
        public Book()
        {
            Overview = string.Empty;
            Images = new List<MediaCover.MediaCover>();
            Links = new List<Links>();
            Genres = new List<string>();
            Ratings = new Ratings();
            Author = new Author();
            AddOptions = new AddBookOptions();
        }

        // These correspond to columns in the Albums table
        // These are metadata entries
        public int AuthorMetadataId { get; set; }
        public string ForeignBookId { get; set; }
        public string ForeignWorkId { get; set; }
        public int GoodreadsId { get; set; }
        public string TitleSlug { get; set; }
        public string Isbn13 { get; set; }
        public string Asin { get; set; }
        public string Title { get; set; }
        public string Language { get; set; }
        public string Overview { get; set; }
        public string Disambiguation { get; set; }
        public string Publisher { get; set; }
        public int PageCount { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
        public List<Links> Links { get; set; }
        public List<string> Genres { get; set; }
        public Ratings Ratings { get; set; }

        // These are Readarr generated/config
        public string CleanTitle { get; set; }
        public bool Monitored { get; set; }
        public DateTime? LastInfoSync { get; set; }
        public DateTime Added { get; set; }
        [MemberwiseEqualityIgnore]
        public AddBookOptions AddOptions { get; set; }

        // These are dynamically queried from other tables
        [MemberwiseEqualityIgnore]
        public LazyLoaded<AuthorMetadata> AuthorMetadata { get; set; }
        [MemberwiseEqualityIgnore]
        public LazyLoaded<Author> Author { get; set; }
        [MemberwiseEqualityIgnore]
        public LazyLoaded<List<BookFile>> BookFiles { get; set; }
        [MemberwiseEqualityIgnore]
        public LazyLoaded<List<SeriesBookLink>> SeriesLinks { get; set; }

        //compatibility properties with old version of Album
        [MemberwiseEqualityIgnore]
        [JsonIgnore]
        public int AuthorId
        {
            get { return Author?.Value?.Id ?? 0; } set { Author.Value.Id = value; }
        }

        public override string ToString()
        {
            return string.Format("[{0}][{1}]", ForeignBookId, Title.NullSafe());
        }

        public override void UseMetadataFrom(Book other)
        {
            ForeignBookId = other.ForeignBookId;
            ForeignWorkId = other.ForeignWorkId;
            GoodreadsId = other.GoodreadsId;
            TitleSlug = other.TitleSlug;
            Isbn13 = other.Isbn13;
            Asin = other.Asin;
            Title = other.Title;
            Language = other.Language;
            Overview = other.Overview.IsNullOrWhiteSpace() ? Overview : other.Overview;
            Disambiguation = other.Disambiguation;
            Publisher = other.Publisher;
            PageCount = other.PageCount;
            ReleaseDate = other.ReleaseDate;
            Images = other.Images.Any() ? other.Images : Images;
            Links = other.Links;
            Genres = other.Genres;
            Ratings = other.Ratings;
            CleanTitle = other.CleanTitle;
        }

        public override void UseDbFieldsFrom(Book other)
        {
            Id = other.Id;
            AuthorMetadataId = other.AuthorMetadataId;
            Monitored = other.Monitored;
            LastInfoSync = other.LastInfoSync;
            Added = other.Added;
            AddOptions = other.AddOptions;
        }

        public override void ApplyChanges(Book other)
        {
            ForeignBookId = other.ForeignBookId;
            ForeignWorkId = other.ForeignWorkId;
            AddOptions = other.AddOptions;
            Monitored = other.Monitored;
        }
    }
}
