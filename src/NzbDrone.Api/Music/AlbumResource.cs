using NzbDrone.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Api.Music
{
    public class AlbumResource
    {
        public string AlbumId { get; set; }
        public string AlbumName { get; set; }
        public bool Monitored { get; set; }
        public DateTime ReleaseDate { get; set; }
        public List<string> Genres { get; set; }
        public string ArtworkUrl { get; set; }

    }

    public static class AlbumResourceMapper
    {
        public static AlbumResource ToResource(this Album model)
        {
            if (model == null) return null;

            return new AlbumResource
            {
                AlbumId = model.ForeignAlbumId,
                Monitored = model.Monitored,
                ReleaseDate = model.ReleaseDate,
                AlbumName = model.Title,
                Genres = model.Genres,
                //ArtworkUrl = model.ArtworkUrl
            };
        }

        public static Album ToModel(this AlbumResource resource)
        {
            if (resource == null) return null;

            return new Album
            {
                ForeignAlbumId = resource.AlbumId,
                Monitored = resource.Monitored,
                ReleaseDate = resource.ReleaseDate,
                Title = resource.AlbumName,
                Genres = resource.Genres,
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
