using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Parser
{
    public interface IParsingService
    {
        Author GetAuthor(string title);
        Author GetAuthorFromTag(string file);
        RemoteBook Map(ParsedBookInfo parsedBookInfo, SearchCriteriaBase searchCriteria = null);
        RemoteBook Map(ParsedBookInfo parsedBookInfo, int authorId, IEnumerable<int> bookIds);
        List<Book> GetBooks(ParsedBookInfo parsedBookInfo, Author author, SearchCriteriaBase searchCriteria = null);

        ParsedBookInfo ParseBookTitleFuzzy(string title);

        // Music stuff here
        Book GetLocalBook(string filename, Author author);
    }

    public class ParsingService : IParsingService
    {
        private readonly IAuthorService _authorService;
        private readonly IBookService _bookService;
        private readonly IMediaFileService _mediaFileService;
        private readonly Logger _logger;

        public ParsingService(IAuthorService authorService,
                              IBookService bookService,
                              IMediaFileService mediaFileService,
                              Logger logger)
        {
            _bookService = bookService;
            _authorService = authorService;
            _mediaFileService = mediaFileService;
            _logger = logger;
        }

        public Author GetAuthor(string title)
        {
            var parsedBookInfo = Parser.ParseBookTitle(title);

            if (parsedBookInfo != null && !parsedBookInfo.AuthorName.IsNullOrWhiteSpace())
            {
                title = parsedBookInfo.AuthorName;
            }

            var authorInfo = _authorService.FindByName(title);

            if (authorInfo == null)
            {
                _logger.Debug("Trying inexact author match for {0}", title);
                authorInfo = _authorService.FindByNameInexact(title);
            }

            return authorInfo;
        }

        public Author GetAuthorFromTag(string file)
        {
            var parsedTrackInfo = Parser.ParseMusicPath(file);

            var author = new Author();

            if (parsedTrackInfo.AuthorMBId.IsNotNullOrWhiteSpace())
            {
                author = _authorService.FindById(parsedTrackInfo.AuthorMBId);

                if (author != null)
                {
                    return author;
                }
            }

            if (parsedTrackInfo == null || parsedTrackInfo.AuthorTitle.IsNullOrWhiteSpace())
            {
                return null;
            }

            author = _authorService.FindByName(parsedTrackInfo.AuthorTitle);

            if (author == null)
            {
                _logger.Debug("Trying inexact author match for {0}", parsedTrackInfo.AuthorTitle);
                author = _authorService.FindByNameInexact(parsedTrackInfo.AuthorTitle);
            }

            return author;
        }

        public RemoteBook Map(ParsedBookInfo parsedBookInfo, SearchCriteriaBase searchCriteria = null)
        {
            var remoteBook = new RemoteBook
            {
                ParsedBookInfo = parsedBookInfo,
            };

            var author = GetAuthor(parsedBookInfo, searchCriteria);

            if (author == null)
            {
                return remoteBook;
            }

            remoteBook.Author = author;
            remoteBook.Books = GetBooks(parsedBookInfo, author, searchCriteria);

            return remoteBook;
        }

        public List<Book> GetBooks(ParsedBookInfo parsedBookInfo, Author author, SearchCriteriaBase searchCriteria = null)
        {
            var bookTitle = parsedBookInfo.BookTitle;
            var result = new List<Book>();

            if (parsedBookInfo.BookTitle == null)
            {
                return new List<Book>();
            }

            Book bookInfo = null;

            if (parsedBookInfo.Discography)
            {
                if (parsedBookInfo.DiscographyStart > 0)
                {
                    return _bookService.AuthorBooksBetweenDates(author,
                        new DateTime(parsedBookInfo.DiscographyStart, 1, 1),
                        new DateTime(parsedBookInfo.DiscographyEnd, 12, 31),
                        false);
                }

                if (parsedBookInfo.DiscographyEnd > 0)
                {
                    return _bookService.AuthorBooksBetweenDates(author,
                        new DateTime(1800, 1, 1),
                        new DateTime(parsedBookInfo.DiscographyEnd, 12, 31),
                        false);
                }

                return _bookService.GetBooksByAuthor(author.Id);
            }

            if (searchCriteria != null)
            {
                bookInfo = searchCriteria.Books.ExclusiveOrDefault(e => e.Title == bookTitle);
            }

            if (bookInfo == null)
            {
                // TODO: Search by Title and Year instead of just Title when matching
                bookInfo = _bookService.FindByTitle(author.AuthorMetadataId, parsedBookInfo.BookTitle);
            }

            if (bookInfo == null)
            {
                _logger.Debug("Trying inexact book match for {0}", parsedBookInfo.BookTitle);
                bookInfo = _bookService.FindByTitleInexact(author.AuthorMetadataId, parsedBookInfo.BookTitle);
            }

            if (bookInfo != null)
            {
                result.Add(bookInfo);
            }
            else
            {
                _logger.Debug("Unable to find {0}", parsedBookInfo);
            }

            return result;
        }

        public RemoteBook Map(ParsedBookInfo parsedBookInfo, int authorId, IEnumerable<int> bookIds)
        {
            return new RemoteBook
            {
                ParsedBookInfo = parsedBookInfo,
                Author = _authorService.GetAuthor(authorId),
                Books = _bookService.GetBooks(bookIds)
            };
        }

        private Author GetAuthor(ParsedBookInfo parsedBookInfo, SearchCriteriaBase searchCriteria)
        {
            Author author = null;

            if (searchCriteria != null)
            {
                if (searchCriteria.Author.CleanName == parsedBookInfo.AuthorName.CleanAuthorName())
                {
                    return searchCriteria.Author;
                }
            }

            author = _authorService.FindByName(parsedBookInfo.AuthorName);

            if (author == null)
            {
                _logger.Debug("Trying inexact author match for {0}", parsedBookInfo.AuthorName);
                author = _authorService.FindByNameInexact(parsedBookInfo.AuthorName);
            }

            if (author == null)
            {
                _logger.Debug("No matching author {0}", parsedBookInfo.AuthorName);
                return null;
            }

            return author;
        }

        public ParsedBookInfo ParseBookTitleFuzzy(string title)
        {
            var bestScore = 0.0;

            Author bestAuthor = null;
            Book bestBook = null;

            var possibleAuthors = _authorService.GetReportCandidates(title);

            foreach (var author in possibleAuthors)
            {
                _logger.Trace($"Trying possible author {author}");

                var authorMatch = title.FuzzyMatch(author.Metadata.Value.Name, 0.5);
                var possibleBooks = _bookService.GetCandidates(author.AuthorMetadataId, title);

                foreach (var book in possibleBooks)
                {
                    var bookMatch = title.FuzzyMatch(book.Title, 0.5);
                    var score = (authorMatch.Item2 + bookMatch.Item2) / 2;

                    _logger.Trace($"Book {book} has score {score}");

                    if (score > bestScore)
                    {
                        bestAuthor = author;
                        bestBook = book;
                    }
                }
            }

            _logger.Trace($"Best match: {bestAuthor} {bestBook}");

            if (bestAuthor != null)
            {
                return Parser.ParseBookTitleWithSearchCriteria(title, bestAuthor, new List<Book> { bestBook });
            }

            return null;
        }

        public Book GetLocalBook(string filename, Author author)
        {
            if (Path.HasExtension(filename))
            {
                filename = Path.GetDirectoryName(filename);
            }

            var tracksInBook = _mediaFileService.GetFilesByAuthor(author.Id)
                .FindAll(s => Path.GetDirectoryName(s.Path) == filename)
                .DistinctBy(s => s.EditionId)
                .ToList();

            return tracksInBook.Count == 1 ? _bookService.GetBook(tracksInBook.First().EditionId) : null;
        }
    }
}
