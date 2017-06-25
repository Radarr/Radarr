using NzbDrone.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Api.REST;
using NzbDrone.Api.Music;
using NzbDrone.Core.MediaCover;

namespace NzbDrone.Api.Albums
{
    public class AlbumResource : RestResource
    {
        
        public string Title { get; set; }
        public string Label { get; set; }
        public bool Monitored { get; set; }
        public string Path { get; set; }
        public int ProfileId { get; set; }
        public Ratings Ratings { get; set; }
        public DateTime ReleaseDate { get; set; }
        public List<string> Genres { get; set; }
        public ArtistResource Artist { get; set; }
        public List<MediaCover> Images { get; set; }

    }

    public static class AlbumResourceMapper
    {
        public static AlbumResource ToResource(this Core.Music.Album model)
        {
            if (model == null) return null;

            return new AlbumResource
            {
                Id = model.Id,
                Label = model.Label,
                Path = model.Path,
                ProfileId = model.ProfileId,
                Monitored = model.Monitored,
                ReleaseDate = model.ReleaseDate,
                Genres = model.Genres,
                Title = model.Title,
                Images = model.Images,
                Ratings = model.Ratings,
                //Genres = model.Genres,
                //ArtworkUrl = model.ArtworkUrl
            };
        }

        public static Album ToModel(this AlbumResource resource)
        {
            if (resource == null) return null;

            return new Core.Music.Album
            {
                Id = resource.Id,
                Label = resource.Label,
                Path = resource.Path,
                Monitored = resource.Monitored,
                ProfileId = resource.ProfileId,
                ReleaseDate = resource.ReleaseDate,
                Genres = resource.Genres,
                Title = resource.Title,
                Images = resource.Images,
                Ratings = resource.Ratings,
                //Genres = resource.Genres,
                //ArtworkUrl = resource.ArtworkUrl
            };
        }

        public static List<AlbumResource> ToResource(this IEnumerable<Album> models)
        {
            return models.Select(ToResource).ToList();
        }

        public static List<Album> ToModel(this IEnumerable<AlbumResource> resources)
        {
            return resources?.Select(ToModel).ToList() ?? new List<Album>();
        }
    }
}
