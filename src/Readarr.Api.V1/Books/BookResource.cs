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
        public int AuthorId { get; set; }
        public string ForeignBookId { get; set; }
        public string TitleSlug { get; set; }
        public bool Monitored { get; set; }
        public bool AnyEditionOk { get; set; }
        public Ratings Ratings { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int PageCount { get; set; }
        public List<string> Genres { get; set; }
        public AuthorResource Author { get; set; }
        public List<MediaCover> Images { get; set; }
        public List<Links> Links { get; set; }
        public BookStatisticsResource Statistics { get; set; }
        public AddBookOptions AddOptions { get; set; }
        public string RemoteCover { get; set; }
        public List<EditionResource> Editions { get; set; }

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

            var selectedEdition = model.Editions?.Value.Where(x => x.Monitored).SingleOrDefault();

            return new BookResource
            {
                Id = model.Id,
                AuthorId = model.AuthorId,
                ForeignBookId = model.ForeignBookId,
                TitleSlug = model.TitleSlug,
                Monitored = model.Monitored,
                AnyEditionOk = model.AnyEditionOk,
                ReleaseDate = model.ReleaseDate,
                PageCount = selectedEdition?.PageCount ?? 0,
                Genres = model.Genres,
                Title = selectedEdition?.Title ?? model.Title,
                Disambiguation = selectedEdition?.Disambiguation,
                Overview = selectedEdition?.Overview,
                Images = selectedEdition?.Images ?? new List<MediaCover>(),
                Links = model.Links.Concat(selectedEdition?.Links ?? new List<Links>()).ToList(),
                Ratings = selectedEdition?.Ratings ?? new Ratings(),
                Author = model.Author?.Value.ToResource(),
                Editions = model.Editions?.Value.ToResource() ?? new List<EditionResource>()
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
                TitleSlug = resource.TitleSlug,
                Title = resource.Title,
                Monitored = resource.Monitored,
                AnyEditionOk = resource.AnyEditionOk,
                Editions = resource.Editions.ToModel(),
                AddOptions = resource.AddOptions,
                Author = author,
                AuthorMetadata = author.Metadata.Value
            };
        }

        public static Book ToModel(this BookResource resource, Book book)
        {
            var updatedBook = resource.ToModel();

            book.ApplyChanges(updatedBook);
            book.Editions = updatedBook.Editions;

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
