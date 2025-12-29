using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Books;
using Radarr.Http;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Books
{
    [V3ApiController("book/lookup")]
    public class BookLookupController : RestController<BookResource>
    {
        private readonly IBookService _bookService;

        public BookLookupController(IBookService bookService)
        {
            _bookService = bookService;
        }

        [NonAction]
        public override ActionResult<BookResource> GetResourceByIdWithErrorHandler(int id)
        {
            throw new NotImplementedException();
        }

        protected override BookResource GetResourceById(int id)
        {
            throw new NotImplementedException();
        }

        [HttpGet("isbn")]
        [Produces("application/json")]
        public ActionResult<BookResource> SearchByIsbn(string isbn)
        {
            var book = _bookService.FindByIsbn(isbn);
            if (book == null)
            {
                return NotFound();
            }

            return book.ToResource();
        }

        [HttpGet("isbn13")]
        [Produces("application/json")]
        public ActionResult<BookResource> SearchByIsbn13(string isbn13)
        {
            var book = _bookService.FindByIsbn13(isbn13);
            if (book == null)
            {
                return NotFound();
            }

            return book.ToResource();
        }

        [HttpGet("asin")]
        [Produces("application/json")]
        public ActionResult<BookResource> SearchByAsin(string asin)
        {
            var book = _bookService.FindByAsin(asin);
            if (book == null)
            {
                return NotFound();
            }

            return book.ToResource();
        }

        [HttpGet("foreignid")]
        [Produces("application/json")]
        public ActionResult<BookResource> SearchByForeignId(string foreignId)
        {
            var book = _bookService.FindByForeignId(foreignId);
            if (book == null)
            {
                return NotFound();
            }

            return book.ToResource();
        }

        [HttpGet]
        [Produces("application/json")]
        public IEnumerable<BookResource> Search([FromQuery] string term)
        {
            // For now, search in local database by title
            // Future: integrate with metadata provider
            var allBooks = _bookService.GetAllBooks();
            var results = new List<BookResource>();

            foreach (var book in allBooks)
            {
                if (book.Title != null &&
                    book.Title.Contains(term, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(book.ToResource());
                }
            }

            return results;
        }
    }
}
