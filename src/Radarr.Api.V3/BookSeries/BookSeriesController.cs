using System.Collections.Generic;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.BookSeries;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.SignalR;
using Radarr.Http;
using Radarr.Http.REST;
using Radarr.Http.REST.Attributes;
using BookSeriesModel = NzbDrone.Core.BookSeries.BookSeries;

namespace Radarr.Api.V3.BookSeries
{
    [V3ApiController]
    public class BookSeriesController : RestControllerWithSignalR<BookSeriesResource, BookSeriesModel>
    {
        private readonly IBookSeriesService _bookSeriesService;

        public BookSeriesController(IBroadcastSignalRMessage signalRBroadcaster,
                                    IBookSeriesService bookSeriesService)
            : base(signalRBroadcaster)
        {
            _bookSeriesService = bookSeriesService;

            PostValidator.RuleFor(s => s.Title).NotEmpty();
        }

        [HttpGet]
        public List<BookSeriesResource> GetBookSeries(int? authorId = null)
        {
            List<BookSeriesModel> bookSeriesList;

            if (authorId.HasValue)
            {
                bookSeriesList = _bookSeriesService.FindByAuthorId(authorId.Value);
            }
            else
            {
                bookSeriesList = _bookSeriesService.GetAllBookSeries();
            }

            return bookSeriesList.ToResource();
        }

        protected override BookSeriesResource GetResourceById(int id)
        {
            var bookSeries = _bookSeriesService.GetBookSeries(id);
            return bookSeries?.ToResource();
        }

        [RestPostById]
        [Consumes("application/json")]
        [Produces("application/json")]
        public ActionResult<BookSeriesResource> AddBookSeries([FromBody] BookSeriesResource bookSeriesResource)
        {
            var bookSeries = _bookSeriesService.AddBookSeries(bookSeriesResource.ToModel());
            return Created(bookSeries.Id);
        }

        [RestPutById]
        [Consumes("application/json")]
        [Produces("application/json")]
        public ActionResult<BookSeriesResource> UpdateBookSeries([FromBody] BookSeriesResource bookSeriesResource)
        {
            var bookSeries = _bookSeriesService.GetBookSeries(bookSeriesResource.Id);
            var updatedBookSeries = _bookSeriesService.UpdateBookSeries(bookSeriesResource.ToModel(bookSeries));
            var resource = updatedBookSeries.ToResource();

            BroadcastResourceChange(ModelAction.Updated, resource);

            return Ok(resource);
        }

        [RestDeleteById]
        public ActionResult DeleteBookSeries(int id)
        {
            _bookSeriesService.DeleteBookSeries(id);
            return NoContent();
        }
    }
}
