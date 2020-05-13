using System.Linq;
using Nancy;
using NzbDrone.Core.Books;
using Readarr.Http.Extensions;

namespace Readarr.Api.V1.AlbumStudio
{
    public class AlbumStudioModule : ReadarrV1Module
    {
        private readonly IAuthorService _authorService;
        private readonly IBookMonitoredService _albumMonitoredService;

        public AlbumStudioModule(IAuthorService authorService, IBookMonitoredService albumMonitoredService)
            : base("/albumstudio")
        {
            _authorService = authorService;
            _albumMonitoredService = albumMonitoredService;
            Post("/", artist => UpdateAll());
        }

        private object UpdateAll()
        {
            //Read from request
            var request = Request.Body.FromJson<AlbumStudioResource>();
            var artistToUpdate = _authorService.GetAuthors(request.Artist.Select(s => s.Id));

            foreach (var s in request.Artist)
            {
                var artist = artistToUpdate.Single(c => c.Id == s.Id);

                if (s.Monitored.HasValue)
                {
                    artist.Monitored = s.Monitored.Value;
                }

                if (request.MonitoringOptions != null && request.MonitoringOptions.Monitor == MonitorTypes.None)
                {
                    artist.Monitored = false;
                }

                _albumMonitoredService.SetBookMonitoredStatus(artist, request.MonitoringOptions);
            }

            return ResponseWithCode("ok", HttpStatusCode.Accepted);
        }
    }
}
