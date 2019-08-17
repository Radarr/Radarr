using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Music;
using Lidarr.Http.REST;
using Newtonsoft.Json;

namespace Lidarr.Api.V1.Artist
{
    public class ArtistResource : RestResource
    {
        //Todo: Sorters should be done completely on the client
        //Todo: Is there an easy way to keep IgnoreArticlesWhenSorting in sync between, Series, History, Missing?
        //Todo: We should get the entire Profile instead of ID and Name separately

        [JsonIgnore]
        public int ArtistMetadataId { get; set; }
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
                ArtistMetadataId = model.ArtistMetadataId,

                ArtistName = model.Name,
                //AlternateTitles
                SortName = model.SortName,

                Status = model.Metadata.Value.Status,
                Overview = model.Metadata.Value.Overview,
                ArtistType = model.Metadata.Value.Type,
                Disambiguation = model.Metadata.Value.Disambiguation,

                Images = model.Metadata.Value.Images.JsonClone(),

                Path = model.Path,
                QualityProfileId = model.QualityProfileId,
                MetadataProfileId = model.MetadataProfileId,
                Links = model.Metadata.Value.Links,

                AlbumFolder = model.AlbumFolder,
                Monitored = model.Monitored,

                LastInfoSync = model.LastInfoSync,

                CleanName = model.CleanName,
                ForeignArtistId = model.Metadata.Value.ForeignArtistId,
                // Root folder path is now calculated from the artist path
                // RootFolderPath = model.RootFolderPath,
                Genres = model.Metadata.Value.Genres,
                Tags = model.Tags,
                Added = model.Added,
                AddOptions = model.AddOptions,
                Ratings = model.Metadata.Value.Ratings,

                Statistics = new ArtistStatisticsResource()
            };
        }

        public static NzbDrone.Core.Music.Artist ToModel(this ArtistResource resource)
        {
            if (resource == null) return null;

            return new NzbDrone.Core.Music.Artist
            {
                Id = resource.Id,

                Metadata = new NzbDrone.Core.Music.ArtistMetadata
                {
                    ForeignArtistId = resource.ForeignArtistId,
                    Name = resource.ArtistName,
                    Status = resource.Status,
                    Overview = resource.Overview,
                    Links = resource.Links,
                    Images = resource.Images,
                    Genres = resource.Genres,
                    Ratings = resource.Ratings,
                    Type = resource.ArtistType
                },
                    
                //AlternateTitles
                SortName = resource.SortName,
                Path = resource.Path,
                QualityProfileId = resource.QualityProfileId,
                MetadataProfileId = resource.MetadataProfileId,


                AlbumFolder = resource.AlbumFolder,
                Monitored = resource.Monitored,

                LastInfoSync = resource.LastInfoSync,
                CleanName = resource.CleanName,
                RootFolderPath = resource.RootFolderPath,

                Tags = resource.Tags,
                Added = resource.Added,
                AddOptions = resource.AddOptions,

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
