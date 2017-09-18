using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NzbDrone.Core.Music;
using Lidarr.Api.V3.Artist;
using Lidarr.Http.REST;
using NzbDrone.Core.MediaCover;

namespace Lidarr.Api.V3.Albums
{
    public class AlbumResource : RestResource
    {
        public string Title { get; set; }
        public int ArtistId { get; set; }
        public List<string> AlbumLabel { get; set; }
        public bool Monitored { get; set; }
        public string Path { get; set; }
        public int ProfileId { get; set; }
        public int Duration { get; set; }
        public string AlbumType { get; set; }
        public Ratings Ratings { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public List<string> Genres { get; set; }
        public ArtistResource Artist { get; set; }
        public List<MediaCover> Images { get; set; }
        public AlbumStatisticsResource Statistics { get; set; }

        //Hiding this so people don't think its usable (only used to set the initial state)
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Grabbed { get; set; }
    }

    public static class EpisodeResourceMapper
    {
        public static AlbumResource ToResource(this Album model)
        {
            if (model == null) return null;

            return new AlbumResource
            {
                Id = model.Id,
                ArtistId = model.ArtistId,
                AlbumLabel = model.Label,
                Path = model.Path,
                ProfileId = model.ProfileId,
                Monitored = model.Monitored,
                ReleaseDate = model.ReleaseDate,
                Genres = model.Genres,
                Title = model.Title,
                Images = model.Images,
                Ratings = model.Ratings,
                Duration = model.Duration,
                AlbumType = model.AlbumType
            };
        }

        public static List<AlbumResource> ToResource(this IEnumerable<Album> models)
        {
            if (models == null) return null;

            return models.Select(ToResource).ToList();
        }
    }
}
