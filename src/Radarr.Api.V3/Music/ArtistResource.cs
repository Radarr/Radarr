using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Music;
using Radarr.Api.V3.MediaItems;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Music
{
    [global::System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S6968", Justification = "Follows existing resource patterns")]
    public class ArtistResource : RestResource, IMediaResource
    {
        public ArtistResource()
        {
        }

        public string Name { get; set; }
        public string SortName { get; set; }
        public string Description { get; set; }
        public string ForeignArtistId { get; set; }
        public string DiscogsId { get; set; }
        public string ArtistType { get; set; }
        public string Status { get; set; }
        public bool Monitored { get; set; }
        public string Path { get; set; }
        public string RootFolderPath { get; set; }
        public int QualityProfileId { get; set; }
        public DateTime Added { get; set; }
        public HashSet<int> Tags { get; set; }
    }

    public static class ArtistResourceMapper
    {
        public static ArtistResource ToResource(this Artist model)
        {
            if (model == null)
            {
                return null;
            }

            return new ArtistResource
            {
                Id = model.Id,
                Name = model.Name,
                SortName = model.SortName,
                Description = model.Description,
                ForeignArtistId = model.ForeignArtistId,
                DiscogsId = model.DiscogsId,
                ArtistType = model.ArtistType,
                Status = model.Status,
                Monitored = model.Monitored,
                Path = model.Path,
                RootFolderPath = model.RootFolderPath,
                QualityProfileId = model.QualityProfileId,
                Added = model.Added,
                Tags = model.Tags
            };
        }

        public static Artist ToModel(this ArtistResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new Artist
            {
                Id = resource.Id,
                Name = resource.Name,
                SortName = resource.SortName,
                Description = resource.Description,
                ForeignArtistId = resource.ForeignArtistId,
                DiscogsId = resource.DiscogsId,
                ArtistType = resource.ArtistType,
                Status = resource.Status,
                Monitored = resource.Monitored,
                Path = resource.Path,
                RootFolderPath = resource.RootFolderPath,
                QualityProfileId = resource.QualityProfileId,
                Tags = resource.Tags ?? new HashSet<int>()
            };
        }

        public static Artist ToModel(this ArtistResource resource, Artist artist)
        {
            var updatedArtist = resource.ToModel();

            artist.Name = updatedArtist.Name;
            artist.SortName = updatedArtist.SortName;
            artist.Description = updatedArtist.Description;
            artist.ForeignArtistId = updatedArtist.ForeignArtistId;
            artist.DiscogsId = updatedArtist.DiscogsId;
            artist.ArtistType = updatedArtist.ArtistType;
            artist.Status = updatedArtist.Status;
            artist.Monitored = updatedArtist.Monitored;
            artist.Path = updatedArtist.Path;
            artist.RootFolderPath = updatedArtist.RootFolderPath;
            artist.QualityProfileId = updatedArtist.QualityProfileId;
            artist.Tags = updatedArtist.Tags;

            return artist;
        }

        public static List<Artist> ToModel(this IEnumerable<ArtistResource> resources)
        {
            return resources.Select(ToModel).ToList();
        }

        public static List<ArtistResource> ToResource(this IEnumerable<Artist> artists)
        {
            return artists.Select(ToResource).ToList();
        }
    }
}
