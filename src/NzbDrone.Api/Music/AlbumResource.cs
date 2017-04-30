using NzbDrone.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Api.Music
{
    public class AlbumResource
    {
        public int AlbumId { get; set; }
        public string AlbumName { get; set; }
        public bool Monitored { get; set; }
        public int Year { get; set; }
        public List<string> Genre { get; set; }
    }

    public static class AlbumResourceMapper
    {
        public static AlbumResource ToResource(this Album model)
        {
            if (model == null) return null;

            return new AlbumResource
            {
                AlbumId = model.AlbumId,
                Monitored = model.Monitored,
                Year = model.Year,
                AlbumName = model.Title
            };
        }

        public static Album ToModel(this AlbumResource resource)
        {
            if (resource == null) return null;

            return new Album
            {
                AlbumId = resource.AlbumId,
                Monitored = resource.Monitored,
                Year = resource.Year,
                Title = resource.AlbumName
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
