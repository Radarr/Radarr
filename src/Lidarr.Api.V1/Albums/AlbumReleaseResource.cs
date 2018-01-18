using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Music;

namespace Lidarr.Api.V1.Albums
{
    public class AlbumReleaseResource
    {
        public string Id { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int TrackCount { get; set; }
        public int MediaCount { get; set; }
        public string Disambiguation { get; set; }
        public List<string> Country { get; set; }
        public List<string> Label { get; set; }
        public string Format { get; set; }
    }

    public static class AlbumReleaseResourceMapper
    {
        public static AlbumReleaseResource ToResource(this AlbumRelease model)
        {
            if (model == null)
            {
                return null;
            }

            return new AlbumReleaseResource
            {
                Id = model.Id,
                ReleaseDate = model.ReleaseDate,
                TrackCount = model.TrackCount,
                MediaCount = model.MediaCount,
                Disambiguation = model.Disambiguation,
                Country = model.Country,
                Label = model.Label,
                Format = model.Format
            };
        }

        public static AlbumRelease ToModel(this AlbumReleaseResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new AlbumRelease
            {
                Id = resource.Id,
                ReleaseDate = resource.ReleaseDate,
                TrackCount = resource.TrackCount,
                MediaCount = resource.MediaCount,
                Disambiguation = resource.Disambiguation,
                Country = resource.Country,
                Label = resource.Label,
                Format = resource.Format
            };
        }

        public static List<AlbumReleaseResource> ToResource(this IEnumerable<AlbumRelease> models)
        {
            return models.Select(ToResource).ToList();
        }

        public static List<AlbumRelease> ToModel(this IEnumerable<AlbumReleaseResource> resources)
        {
            return resources.Select(ToModel).ToList();
        }
    }
}
