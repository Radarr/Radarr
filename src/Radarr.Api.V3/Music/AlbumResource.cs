using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Music;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Music
{
    [global::System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S6968", Justification = "Follows existing resource patterns")]
    public class AlbumResource : RestResource
    {
        public AlbumResource()
        {
            Monitored = true;
        }

        public int? ArtistId { get; set; }
        public string Title { get; set; }
        public string SortTitle { get; set; }
        public string Description { get; set; }
        public string ForeignAlbumId { get; set; }
        public string DiscogsId { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string AlbumType { get; set; }
        public bool Monitored { get; set; }
        public bool EffectivelyMonitored { get; set; }
        public int QualityProfileId { get; set; }
        public string Path { get; set; }
        public string RootFolderPath { get; set; }
        public DateTime Added { get; set; }
        public HashSet<int> Tags { get; set; }
        public DateTime? LastSearchTime { get; set; }

        public bool? HasFile { get; set; }
        public long? SizeOnDisk { get; set; }
        public MusicStatisticsResource Statistics { get; set; }
    }

    public static class AlbumResourceMapper
    {
        public static AlbumResource ToResource(this Album model)
        {
            if (model == null)
            {
                return null;
            }

            return new AlbumResource
            {
                Id = model.Id,
                ArtistId = model.ArtistId,
                Title = model.Title,
                SortTitle = model.SortTitle,
                Description = model.Description,
                ForeignAlbumId = model.ForeignAlbumId,
                DiscogsId = model.DiscogsId,
                ReleaseDate = model.ReleaseDate,
                AlbumType = model.AlbumType,
                Monitored = model.Monitored,
                QualityProfileId = model.QualityProfileId,
                Path = model.Path,
                RootFolderPath = model.RootFolderPath,
                Added = model.Added,
                Tags = model.Tags,
                LastSearchTime = model.LastSearchTime
            };
        }

        public static Album ToModel(this AlbumResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new Album
            {
                Id = resource.Id,
                ArtistId = resource.ArtistId,
                Title = resource.Title,
                SortTitle = resource.SortTitle,
                Description = resource.Description,
                ForeignAlbumId = resource.ForeignAlbumId,
                DiscogsId = resource.DiscogsId,
                ReleaseDate = resource.ReleaseDate,
                AlbumType = resource.AlbumType,
                Monitored = resource.Monitored,
                QualityProfileId = resource.QualityProfileId,
                Path = resource.Path,
                RootFolderPath = resource.RootFolderPath,
                Tags = resource.Tags ?? new HashSet<int>()
            };
        }

        public static Album ToModel(this AlbumResource resource, Album album)
        {
            var updatedAlbum = resource.ToModel();

            album.ArtistId = updatedAlbum.ArtistId;
            album.Title = updatedAlbum.Title;
            album.SortTitle = updatedAlbum.SortTitle;
            album.Description = updatedAlbum.Description;
            album.ForeignAlbumId = updatedAlbum.ForeignAlbumId;
            album.DiscogsId = updatedAlbum.DiscogsId;
            album.ReleaseDate = updatedAlbum.ReleaseDate;
            album.AlbumType = updatedAlbum.AlbumType;
            album.Monitored = updatedAlbum.Monitored;
            album.QualityProfileId = updatedAlbum.QualityProfileId;
            album.Path = updatedAlbum.Path;
            album.RootFolderPath = updatedAlbum.RootFolderPath;
            album.Tags = updatedAlbum.Tags;

            return album;
        }

        public static List<AlbumResource> ToResource(this IEnumerable<Album> albums)
        {
            return albums.Select(ToResource).ToList();
        }

        public static List<Album> ToModel(this IEnumerable<AlbumResource> resources)
        {
            return resources.Select(ToModel).ToList();
        }
    }
}
