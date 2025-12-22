using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Books;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Books
{
    public class BookResource : RestResource
    {
        public BookResource()
        {
            Monitored = true;
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
                Title = model.Title,
                SortTitle = model.SortTitle,
                Description = model.Description,
                ForeignBookId = model.ForeignBookId,
                Isbn = model.Isbn,
                Isbn13 = model.Isbn13,
                Asin = model.Asin,
                PageCount = model.PageCount,
                ReleaseDate = model.ReleaseDate,
                Publisher = model.Publisher,
                Language = model.Language,
                Monitored = model.Monitored,
                QualityProfileId = model.QualityProfileId,
                Path = model.Path,
                RootFolderPath = model.RootFolderPath,
                Added = model.Added,
                Tags = model.Tags,
                LastSearchTime = model.LastSearchTime,
                AuthorId = model.AuthorId,
                SeriesId = model.SeriesId,
                SeriesPosition = model.SeriesPosition
            };
        }

        public static Book ToModel(this BookResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new Book
            {
                Id = resource.Id,
                Title = resource.Title,
                SortTitle = resource.SortTitle,
                Description = resource.Description,
                ForeignBookId = resource.ForeignBookId,
                Isbn = resource.Isbn,
                Isbn13 = resource.Isbn13,
                Asin = resource.Asin,
                PageCount = resource.PageCount,
                ReleaseDate = resource.ReleaseDate,
                Publisher = resource.Publisher,
                Language = resource.Language,
                Monitored = resource.Monitored,
                QualityProfileId = resource.QualityProfileId,
                Path = resource.Path,
                RootFolderPath = resource.RootFolderPath,
                Tags = resource.Tags ?? new HashSet<int>(),
                AuthorId = resource.AuthorId,
                SeriesId = resource.SeriesId,
                SeriesPosition = resource.SeriesPosition
            };
        }

        public static Book ToModel(this BookResource resource, Book book)
        {
            var updatedBook = resource.ToModel();

            book.Title = updatedBook.Title;
            book.SortTitle = updatedBook.SortTitle;
            book.Description = updatedBook.Description;
            book.ForeignBookId = updatedBook.ForeignBookId;
            book.Isbn = updatedBook.Isbn;
            book.Isbn13 = updatedBook.Isbn13;
            book.Asin = updatedBook.Asin;
            book.PageCount = updatedBook.PageCount;
            book.ReleaseDate = updatedBook.ReleaseDate;
            book.Publisher = updatedBook.Publisher;
            book.Language = updatedBook.Language;
            book.Monitored = updatedBook.Monitored;
            book.QualityProfileId = updatedBook.QualityProfileId;
            book.Path = updatedBook.Path;
            book.RootFolderPath = updatedBook.RootFolderPath;
            book.Tags = updatedBook.Tags;
            book.AuthorId = updatedBook.AuthorId;
            book.SeriesId = updatedBook.SeriesId;
            book.SeriesPosition = updatedBook.SeriesPosition;

            return book;
        }

        public static List<BookResource> ToResource(this IEnumerable<Book> books)
        {
            return books.Select(ToResource).ToList();
        }

        public static List<Book> ToModel(this IEnumerable<BookResource> resources)
        {
            return resources.Select(ToModel).ToList();
        }
    }
}
