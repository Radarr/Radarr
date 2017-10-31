using System.Linq;
using Nancy;
using NzbDrone.Core.Music;
using Lidarr.Http.Extensions;

namespace Lidarr.Api.V1.AlbumStudio
{
    public class AlbumStudioModule : LidarrV1Module
    {
        private readonly IArtistService _artistService;
        private readonly IAlbumMonitoredService _episodeMonitoredService;

        public AlbumStudioModule(IArtistService artistService, IAlbumMonitoredService episodeMonitoredService)
            : base("/albumstudio")
        {
            _artistService = artistService;
            _episodeMonitoredService = episodeMonitoredService;
            Post["/"] = artist => UpdateAll();
        }

        private Response UpdateAll()
        {
            //Read from request
            var request = Request.Body.FromJson<AlbumStudioResource>();
            var artistToUpdate = _artistService.GetArtists(request.Artist.Select(s => s.Id));

            foreach (var s in request.Artist)
            {
                var artist = artistToUpdate.Single(c => c.Id == s.Id);

                if (s.Monitored.HasValue)
                {
                    artist.Monitored = s.Monitored.Value;
                }

                if (s.Albums != null && s.Albums.Any())
                {
                    foreach (var artistAlbum in artist.Albums)
                    {
                        var album = s.Albums.FirstOrDefault(c => c.Id == artistAlbum.Id);

                        if (album != null)
                        {
                            artistAlbum.Monitored = album.Monitored;
                        }
                    }
                }

                _episodeMonitoredService.SetAlbumMonitoredStatus(artist, request.MonitoringOptions);
            }

            return "ok".AsResponse(HttpStatusCode.Accepted);
        }
    }
}
