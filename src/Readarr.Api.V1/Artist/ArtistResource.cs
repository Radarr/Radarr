using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books;
using NzbDrone.Core.MediaCover;
using Readarr.Http.REST;

namespace Readarr.Api.V1.Artist
{
    public class ArtistResource : RestResource
    {
        //Todo: Sorters should be done completely on the client
        //Todo: Is there an easy way to keep IgnoreArticlesWhenSorting in sync between, Series, History, Missing?
        //Todo: We should get the entire Profile instead of ID and Name separately
        [JsonIgnore]
        public int ArtistMetadataId { get; set; }
        public AuthorStatusType Status { get; set; }

        public bool Ended => Status == AuthorStatusType.Ended;

        public string ArtistName { get; set; }
        public string ForeignAuthorId { get; set; }
        public int GoodreadsId { get; set; }
        public string TitleSlug { get; set; }
        public string Overview { get; set; }
        public string ArtistType { get; set; }
        public string Disambiguation { get; set; }
        public List<Links> Links { get; set; }

        public Book NextAlbum { get; set; }
        public Book LastAlbum { get; set; }

        public List<MediaCover> Images { get; set; }

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
        public AddAuthorOptions AddOptions { get; set; }
        public Ratings Ratings { get; set; }

        public ArtistStatisticsResource Statistics { get; set; }
    }

    public static class ArtistResourceMapper
    {
        public static ArtistResource ToResource(this NzbDrone.Core.Books.Author model)
        {
            if (model == null)
            {
                return null;
            }

            return new ArtistResource
            {
                Id = model.Id,
                ArtistMetadataId = model.AuthorMetadataId,

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

                Monitored = model.Monitored,

                CleanName = model.CleanName,
                ForeignAuthorId = model.Metadata.Value.ForeignAuthorId,
                GoodreadsId = model.Metadata.Value.GoodreadsId,
                TitleSlug = model.Metadata.Value.TitleSlug,

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

        public static NzbDrone.Core.Books.Author ToModel(this ArtistResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new NzbDrone.Core.Books.Author
            {
                Id = resource.Id,

                Metadata = new NzbDrone.Core.Books.AuthorMetadata
                {
                    ForeignAuthorId = resource.ForeignAuthorId,
                    GoodreadsId = resource.GoodreadsId,
                    TitleSlug = resource.TitleSlug,
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

                Monitored = resource.Monitored,

                CleanName = resource.CleanName,
                RootFolderPath = resource.RootFolderPath,

                Tags = resource.Tags,
                Added = resource.Added,
                AddOptions = resource.AddOptions,
            };
        }

        public static NzbDrone.Core.Books.Author ToModel(this ArtistResource resource, NzbDrone.Core.Books.Author artist)
        {
            var updatedArtist = resource.ToModel();

            artist.ApplyChanges(updatedArtist);

            return artist;
        }

        public static List<ArtistResource> ToResource(this IEnumerable<NzbDrone.Core.Books.Author> artist)
        {
            return artist.Select(ToResource).ToList();
        }

        public static List<NzbDrone.Core.Books.Author> ToModel(this IEnumerable<ArtistResource> resources)
        {
            return resources.Select(ToModel).ToList();
        }
    }
}
