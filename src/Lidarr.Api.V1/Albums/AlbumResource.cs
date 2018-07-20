using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NzbDrone.Core.Music;
using Lidarr.Api.V1.Artist;
using Lidarr.Http.REST;
using NzbDrone.Core.MediaCover;

namespace Lidarr.Api.V1.Albums
{
    public class AlbumResource : RestResource
    {
        public string Title { get; set; }
        public string Disambiguation { get; set; }
        public int ArtistId { get; set; }
        public List<string> AlbumLabel { get; set; }
        public string ForeignAlbumId { get; set; }
        public bool Monitored { get; set; }
        public int ProfileId { get; set; }
        public int Duration { get; set; }
        public string AlbumType { get; set; }
        public List<string> SecondaryTypes { get; set; }
        public int MediumCount
        {
            get
            {
                if (Media == null)
                {
                    return 0;
                }

                return Media.Where(s => s.MediumNumber > 0).Count();
            }
        }
        public Ratings Ratings { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public AlbumRelease CurrentRelease { get; set; }
        public List<AlbumReleaseResource> Releases { get; set; }
        public List<string> Genres { get; set; }
        public List<MediumResource> Media { get; set; }
        public ArtistResource Artist { get; set; }
        public List<MediaCover> Images { get; set; }
        public AlbumStatisticsResource Statistics { get; set; }

        public string RemoteCover { get; set; }

        //Hiding this so people don't think its usable (only used to set the initial state)
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Grabbed { get; set; }
    }

    public static class AlbumResourceMapper
    {
        public static AlbumResource ToResource(this Album model)
        {
            if (model == null) return null;

            return new AlbumResource
            {
                Id = model.Id,
                ArtistId = model.ArtistId,
                AlbumLabel = model.Label,
                ForeignAlbumId = model.ForeignAlbumId,
                ProfileId = model.ProfileId,
                Monitored = model.Monitored,
                ReleaseDate = model.ReleaseDate,
                Genres = model.Genres,
                Title = model.Title,
                Disambiguation = model.Disambiguation,
                Images = model.Images,
                Ratings = model.Ratings,
                Duration = model.Duration,
                AlbumType = model.AlbumType,
                SecondaryTypes = model.SecondaryTypes.Select(s => s.Name).ToList(),
                Media = model.Media.ToResource(),
                CurrentRelease = model.CurrentRelease,
                Releases = model.Releases.ToResource(),
                Artist = model.Artist.ToResource()
            };
        }

        public static Album ToModel(this AlbumResource resource)
        {
            if (resource == null) return null;

            return new Album
            {
                Id = resource.Id,
                ForeignAlbumId = resource.ForeignAlbumId,
                Title = resource.Title,
                Disambiguation = resource.Disambiguation,
                Images = resource.Images,
                Monitored = resource.Monitored,
                CurrentRelease = resource.CurrentRelease
            };
        }

        public static Album ToModel(this AlbumResource resource, Album album)
        {
            var updatedAlbum = resource.ToModel();

            album.ApplyChanges(updatedAlbum);

            return album;
        }

        public static List<AlbumResource> ToResource(this IEnumerable<Album> models)
        {
            return models?.Select(ToResource).ToList();
        }

        public static List<Album> ToModel(this IEnumerable<AlbumResource> resources)
        {
            return resources.Select(ToModel).ToList();
        }
    }
}
