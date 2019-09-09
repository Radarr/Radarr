using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Music;

namespace Lidarr.Api.V1.Albums
{
    public class AlbumReleaseResource
    {
        public int Id { get; set; }
        public int AlbumId { get; set; }
        public string ForeignReleaseId { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public int Duration { get; set; }
        public int TrackCount { get; set; }
        public List<MediumResource> Media { get; set; }
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
        public string Disambiguation { get; set; }
        public List<string> Country { get; set; }
        public List<string> Label { get; set; }
        public string Format { get; set; }
        public bool Monitored { get; set; }
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
                AlbumId = model.AlbumId,
                ForeignReleaseId = model.ForeignReleaseId,
                Title = model.Title,
                Status = model.Status,
                Duration = model.Duration,
                TrackCount = model.TrackCount,
                Media = model.Media.ToResource(),
                Disambiguation = model.Disambiguation,
                Country = model.Country,
                Label = model.Label,
                Monitored = model.Monitored,
                Format = string.Join(", ",
                                     model.Media.OrderBy(x => x.Number)
                                     .GroupBy(x => x.Format)
                                     .Select(g => MediaFormatHelper(g.Key, g.Count()))
                                     .ToList())

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
                AlbumId = resource.AlbumId,
                ForeignReleaseId = resource.ForeignReleaseId,
                Title = resource.Title,
                Status = resource.Status,
                Duration = resource.Duration,
                Label = resource.Label,
                Disambiguation = resource.Disambiguation,
                Country = resource.Country,
                Media = resource.Media.ToModel(),
                TrackCount = resource.TrackCount,
                Monitored = resource.Monitored
            };
        }

        private static string MediaFormatHelper(string name, int count)
        {
            return count == 1 ? name : string.Join("x", new List<string> {count.ToString(), name});
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
