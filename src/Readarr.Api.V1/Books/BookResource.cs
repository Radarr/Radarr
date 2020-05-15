using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NzbDrone.Core.Books;
using NzbDrone.Core.MediaCover;
using Readarr.Api.V1.Author;
using Readarr.Api.V1.BookFiles;
using Readarr.Http.REST;

namespace Readarr.Api.V1.Books
{
    public class BookResource : RestResource
    {
        public string Title { get; set; }
        public string Disambiguation { get; set; }
        public string Overview { get; set; }
        public string Publisher { get; set; }
        public string Language { get; set; }
        public int AuthorId { get; set; }
        public string ForeignBookId { get; set; }
        public int GoodreadsId { get; set; }
        public string TitleSlug { get; set; }
        public string Isbn { get; set; }
        public string Asin { get; set; }
        public bool Monitored { get; set; }
        public Ratings Ratings { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public List<string> Genres { get; set; }
        public AuthorResource Author { get; set; }
        public List<MediaCover> Images { get; set; }
        public List<Links> Links { get; set; }
        public BookStatisticsResource Statistics { get; set; }
        public AddBookOptions AddOptions { get; set; }
        public string RemoteCover { get; set; }

        //Hiding this so people don't think its usable (only used to set the initial state)
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Grabbed { get; set; }
    }

    public static class BookResourceMapper
    {
        public static BookResource ToResource(this Book model)
        {
            if (model == null)
            {
                return null;
            }

            return new BookResource
            {
                Id = model.Id,
                AuthorId = model.AuthorId,
                ForeignBookId = model.ForeignBookId,
                GoodreadsId = model.GoodreadsId,
                TitleSlug = model.TitleSlug,
                Asin = model.Asin,
                Isbn = model.Isbn13,
                Monitored = model.Monitored,
                ReleaseDate = model.ReleaseDate,
                Genres = model.Genres,
                Title = model.Title,
                Disambiguation = model.Disambiguation,
                Overview = model.Overview,
                Publisher = model.Publisher,
                Language = model.Language,
                Images = model.Images,
                Links = model.Links,
                Ratings = model.Ratings,
                Author = model.Author?.Value.ToResource()
            };
        }

        public static Book ToModel(this BookResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            var author = resource.Author?.ToModel() ?? new NzbDrone.Core.Books.Author();

            return new Book
            {
                Id = resource.Id,
                ForeignBookId = resource.ForeignBookId,
                GoodreadsId = resource.GoodreadsId,
                TitleSlug = resource.TitleSlug,
                Asin = resource.Asin,
                Isbn13 = resource.Isbn,
                Title = resource.Title,
                Disambiguation = resource.Disambiguation,
                Overview = resource.Overview,
                Publisher = resource.Publisher,
                Language = resource.Language,
                Images = resource.Images,
                Monitored = resource.Monitored,
                AddOptions = resource.AddOptions,
                Author = author,
                AuthorMetadata = author.Metadata.Value
            };
        }

        public static Book ToModel(this BookResource resource, Book book)
        {
            var updatedBook = resource.ToModel();

            book.ApplyChanges(updatedBook);

            return book;
        }

        public static List<BookResource> ToResource(this IEnumerable<Book> models)
        {
            return models?.Select(ToResource).ToList();
        }

        public static List<Book> ToModel(this IEnumerable<BookResource> resources)
        {
            return resources.Select(ToModel).ToList();
        }
    }
}
