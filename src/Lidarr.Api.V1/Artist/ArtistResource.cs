using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Music;
using Lidarr.Api.V1.Albums;
using Lidarr.Http.REST;

namespace Lidarr.Api.V1.Artist
{
    public class ArtistResource : RestResource
    {
        //Todo: Sorters should be done completely on the client
        //Todo: Is there an easy way to keep IgnoreArticlesWhenSorting in sync between, Series, History, Missing?
        //Todo: We should get the entire Profile instead of ID and Name separately

        public ArtistStatusType Status { get; set; }

        public bool Ended => Status == ArtistStatusType.Ended;

        public DateTime? LastInfoSync { get; set; }

        public string ArtistName { get; set; }
        public string ForeignArtistId { get; set; }
        public string MBId { get; set; }
        public int TADBId { get; set; }
        public int DiscogsId { get; set; }
        public string AllMusicId { get; set; }
        public string Overview { get; set; }
        public string ArtistType { get; set; }
        public string Disambiguation { get; set; }
        public List<Links> Links { get; set; }
        
        public Album NextAlbum { get; set; }
        public Album LastAlbum { get; set; }

        public List<MediaCover> Images { get; set; }
        public List<Member> Members { get; set; }

        public string RemotePoster { get; set; }


        //View & Edit
        public string Path { get; set; }
        public int QualityProfileId { get; set; }
        public int LanguageProfileId { get; set; }
        public int MetadataProfileId { get; set; }

        //Editing Only
        public bool AlbumFolder { get; set; }
        public bool Monitored { get; set; }

        public string RootFolderPath { get; set; }
        public List<string> Genres { get; set; }
        public string CleanName { get; set; }
        public string SortName { get; set; }
        public HashSet<int> Tags { get; set; }
        public DateTime Added { get; set; }
        public AddArtistOptions AddOptions { get; set; }
        public Ratings Ratings { get; set; }

        public ArtistStatisticsResource Statistics { get; set; }
    }

    public static class ArtistResourceMapper
    {
        public static ArtistResource ToResource(this NzbDrone.Core.Music.Artist model)
        {
            if (model == null) return null;

            return new ArtistResource
            {
                Id = model.Id,

                ArtistName = model.Name,
                //AlternateTitles
                SortName = model.SortName,

                Status = model.Status,
                Overview = model.Overview,
                ArtistType = model.ArtistType,
                Disambiguation = model.Disambiguation,

                Images = model.Images,

                Path = model.Path,
                QualityProfileId = model.ProfileId,
                LanguageProfileId = model.LanguageProfileId,
                MetadataProfileId = model.MetadataProfileId,
                Links = model.Links,

                AlbumFolder = model.AlbumFolder,
                Monitored = model.Monitored,

                LastInfoSync = model.LastInfoSync,

                CleanName = model.CleanName,
                ForeignArtistId = model.ForeignArtistId,
                RootFolderPath = model.RootFolderPath,
                Genres = model.Genres,
                Tags = model.Tags,
                Added = model.Added,
                AddOptions = model.AddOptions,
                Ratings = model.Ratings
            };
        }

        public static NzbDrone.Core.Music.Artist ToModel(this ArtistResource resource)
        {
            if (resource == null) return null;

            return new NzbDrone.Core.Music.Artist
            {
                Id = resource.Id,

                Name = resource.ArtistName,
                //AlternateTitles
                SortName = resource.SortName,

                Status = resource.Status,
                Overview = resource.Overview,

                Images = resource.Images,

                Path = resource.Path,
                ProfileId = resource.QualityProfileId,
                LanguageProfileId = resource.LanguageProfileId,
                MetadataProfileId = resource.MetadataProfileId,
                Links = resource.Links,

                AlbumFolder = resource.AlbumFolder,
                Monitored = resource.Monitored,

                LastInfoSync = resource.LastInfoSync,
                ArtistType = resource.ArtistType,
                CleanName = resource.CleanName,
                ForeignArtistId = resource.ForeignArtistId,
                RootFolderPath = resource.RootFolderPath,
                Genres = resource.Genres,
                Tags = resource.Tags,
                Added = resource.Added,
                AddOptions = resource.AddOptions,
                Ratings = resource.Ratings
            };
        }

        public static NzbDrone.Core.Music.Artist ToModel(this ArtistResource resource, NzbDrone.Core.Music.Artist artist)
        {
            var updatedArtist = resource.ToModel();

            artist.ApplyChanges(updatedArtist);

            return artist;
        }

        public static List<ArtistResource> ToResource(this IEnumerable<NzbDrone.Core.Music.Artist> artist)
        {
            return artist.Select(ToResource).ToList();
        }

        public static List<NzbDrone.Core.Music.Artist> ToModel(this IEnumerable<ArtistResource> resources)
        {
            return resources.Select(ToModel).ToList();
        }
    }
}
