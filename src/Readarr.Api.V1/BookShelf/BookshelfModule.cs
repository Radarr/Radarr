using System.Linq;
using Nancy;
using NzbDrone.Core.Books;
using Readarr.Http.Extensions;

namespace Readarr.Api.V1.Bookshelf
{
    public class BookshelfModule : ReadarrV1Module
    {
        private readonly IAuthorService _authorService;
        private readonly IBookMonitoredService _bookMonitoredService;

        public BookshelfModule(IAuthorService authorService, IBookMonitoredService bookMonitoredService)
            : base("/bookshelf")
        {
            _authorService = authorService;
            _bookMonitoredService = bookMonitoredService;
            Post("/", author => UpdateAll());
        }

        private object UpdateAll()
        {
            //Read from request
            var request = Request.Body.FromJson<BookshelfResource>();
            var authorToUpdate = _authorService.GetAuthors(request.Authors.Select(s => s.Id));

            foreach (var s in request.Authors)
            {
                var author = authorToUpdate.Single(c => c.Id == s.Id);

                if (s.Monitored.HasValue)
                {
                    author.Monitored = s.Monitored.Value;
                }

                if (request.MonitoringOptions != null && request.MonitoringOptions.Monitor == MonitorTypes.None)
                {
                    author.Monitored = false;
                }

                _bookMonitoredService.SetBookMonitoredStatus(author, request.MonitoringOptions);
            }

            return ResponseWithCode("ok", HttpStatusCode.Accepted);
        }
    }
}
