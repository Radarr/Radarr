using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.BookSeries;
using Radarr.Http;
using Radarr.Http.REST;

namespace Radarr.Api.V3.BookSeries
{
    [V3ApiController("bookseries/lookup")]
    public class BookSeriesLookupController : RestController<BookSeriesResource>
    {
        private readonly IBookSeriesService _bookSeriesService;

        public BookSeriesLookupController(IBookSeriesService bookSeriesService)
        {
            _bookSeriesService = bookSeriesService;
        }

        [NonAction]
        public override ActionResult<BookSeriesResource> GetResourceByIdWithErrorHandler(int id)
        {
            throw new NotImplementedException();
        }

        protected override BookSeriesResource GetResourceById(int id)
        {
            throw new NotImplementedException();
        }

        [HttpGet("foreignid")]
        [Produces("application/json")]
        public ActionResult<BookSeriesResource> SearchByForeignId(string foreignId)
        {
            var bookSeries = _bookSeriesService.FindByForeignId(foreignId);
            if (bookSeries == null)
            {
                return NotFound();
            }

            return bookSeries.ToResource();
        }

        [HttpGet("title")]
        [Produces("application/json")]
        public ActionResult<BookSeriesResource> SearchByTitle(string title)
        {
            var bookSeries = _bookSeriesService.FindByTitle(title);
            if (bookSeries == null)
            {
                return NotFound();
            }

            return bookSeries.ToResource();
        }

        [HttpGet("author")]
        [Produces("application/json")]
        public IEnumerable<BookSeriesResource> SearchByAuthor(int authorId)
        {
            var bookSeriesList = _bookSeriesService.FindByAuthorId(authorId);
            return bookSeriesList.ToResource();
        }

        [HttpGet]
        [Produces("application/json")]
        public IEnumerable<BookSeriesResource> Search([FromQuery] string term)
        {
            var allBookSeries = _bookSeriesService.GetAllBookSeries();
            var results = new List<BookSeriesResource>();

            foreach (var bookSeries in allBookSeries)
            {
                if (bookSeries.Title != null &&
                    bookSeries.Title.Contains(term, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(bookSeries.ToResource());
                }
            }

            return results;
        }
    }
}
