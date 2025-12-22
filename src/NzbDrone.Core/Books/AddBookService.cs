using System;
using System.Collections.Generic;
using System.IO;
using FluentValidation;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Organizer;

namespace NzbDrone.Core.Books
{
    public interface IAddBookService
    {
        Book AddBook(Book newBook);
        List<Book> AddBooks(List<Book> newBooks, bool ignoreErrors = false);
    }

    public class AddBookService : IAddBookService
    {
        private readonly IBookService _bookService;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly IAddBookValidator _addBookValidator;
        private readonly Logger _logger;

        public AddBookService(IBookService bookService,
                              IBuildFileNames fileNameBuilder,
                              IAddBookValidator addBookValidator,
                              Logger logger)
        {
            _bookService = bookService;
            _fileNameBuilder = fileNameBuilder;
            _addBookValidator = addBookValidator;
            _logger = logger;
        }

        public Book AddBook(Book newBook)
        {
            Ensure.That(newBook, () => newBook).IsNotNull();

            newBook = SetPropertiesAndValidate(newBook);

            _logger.Info("Adding Book {0} Path: [{1}]", newBook, newBook.Path.SanitizeForLog());

            _bookService.AddBook(newBook);

            return newBook;
        }

        public List<Book> AddBooks(List<Book> newBooks, bool ignoreErrors = false)
        {
            var added = DateTime.UtcNow;
            var booksToAdd = new List<Book>();

            foreach (var b in newBooks)
            {
                if (b.Path.IsNullOrWhiteSpace())
                {
                    _logger.Info("Adding Book {0} Root Folder Path: [{1}]", b, b.RootFolderPath.SanitizeForLog());
                }
                else
                {
                    _logger.Info("Adding Book {0} Path: [{1}]", b, b.Path.SanitizeForLog());
                }

                try
                {
                    var book = SetPropertiesAndValidate(b);
                    book.Added = added;
                    booksToAdd.Add(book);
                }
                catch (ValidationException ex)
                {
                    if (!ignoreErrors)
                    {
                        throw;
                    }

                    _logger.Debug("Book {0} was not added due to validation failures. {1}", b.Title, ex.Message);
                }
            }

            return _bookService.AddBooks(booksToAdd);
        }

        private Book SetPropertiesAndValidate(Book newBook)
        {
            if (string.IsNullOrWhiteSpace(newBook.Path))
            {
                var folderName = GetBookFolder(newBook);
                newBook.Path = Path.Combine(newBook.RootFolderPath, folderName);
            }

            if (string.IsNullOrWhiteSpace(newBook.SortTitle))
            {
                newBook.SortTitle = newBook.Title?.ToLowerInvariant();
            }

            newBook.Added = DateTime.UtcNow;

            var validationResult = _addBookValidator.Validate(newBook);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            return newBook;
        }

        private string GetBookFolder(Book book)
        {
            var title = book.Title ?? "Unknown";
            var year = book.ReleaseDate?.Year.ToString() ?? "Unknown";
            return FileNameBuilder.CleanFileName($"{title} ({year})");
        }
    }
}
