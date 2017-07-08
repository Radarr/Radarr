using NzbDrone.Api.REST;
using NzbDrone.Api.Series;
using NzbDrone.Api.Albums;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Api.Music
{
    public class ArtistResource : RestResource
    {
        public ArtistResource()
        {
            Monitored = true;
        }


        //View Only
        public string Name { get; set; }
        public string ForeignArtistId { get; set; }
        public string MBId { get; set; }
        public int TADBId { get; set; }
        public int DiscogsId { get; set; }
        public string AllMusicId { get; set; }
        public string Overview { get; set; }

        public int? AlbumCount{ get; set; }
        public int? TotalTrackCount { get; set; }
        public int? TrackCount { get; set; }
        public int? TrackFileCount { get; set; }
        public long? SizeOnDisk { get; set; }
        //public SeriesStatusType Status { get; set; }
        
        public List<MediaCover> Images { get; set; }
        public List<Member> Members { get; set; }

        public string RemotePoster { get; set; }
        public List<AlbumResource> Albums { get; set; }
        

        //View & Edit
        public string Path { get; set; }
        public int ProfileId { get; set; }

        //Editing Only
        public bool AlbumFolder { get; set; }
        public bool Monitored { get; set; }

        public string RootFolderPath { get; set; }
        //public string Certification { get; set; }
        public List<string> Genres { get; set; }
        public string CleanName { get; set; }
        public HashSet<int> Tags { get; set; }
        public DateTime Added { get; set; }
        public AddArtistOptions AddOptions { get; set; }
        public Ratings Ratings { get; set; }
        public string NameSlug { get; set; }
    }

    public static class ArtistResourceMapper
    {
        public static ArtistResource ToResource(this Core.Music.Artist model)
        {
            if (model == null) return null;

            return new ArtistResource
            {
                Id = model.Id,
                MBId = model.MBId,
                TADBId = model.TADBId,
                DiscogsId = model.DiscogsId,
                AllMusicId = model.AMId,
                Name = model.Name,
                CleanName = model.CleanName,
                //AlternateTitles
                //SortTitle = resource.SortTitle,

                //TotalTrackCount
                //TrackCount
                //TrackFileCount
                //SizeOnDisk
                //Status = resource.Status,
                Overview = model.Overview,
                //NextAiring
                //PreviousAiring
                //Network = resource.Network,
                //AirTime = resource.AirTime,
                Images = model.Images,
                Members = model.Members,
                //Albums = model.Albums.ToResource(),
                //Year = resource.Year,

                Path = model.Path,
                ProfileId = model.ProfileId,

                Monitored = model.Monitored,
                AlbumFolder = model.AlbumFolder,

                //UseSceneNumbering = resource.UseSceneNumbering,
                //Runtime = resource.Runtime,
                //TvdbId = resource.TvdbId,
                //TvRageId = resource.TvRageId,
                //TvMazeId = resource.TvMazeId,
                //FirstAired = resource.FirstAired,
                //LastInfoSync = resource.LastInfoSync,
                //SeriesType = resource.SeriesType,
                ForeignArtistId = model.ForeignArtistId,
                NameSlug = model.NameSlug,

                RootFolderPath = model.RootFolderPath,
                Genres = model.Genres,
                Tags = model.Tags,
                Added = model.Added,
                AddOptions = model.AddOptions,
                Ratings = model.Ratings,
            };
        }

        public static Core.Music.Artist ToModel(this ArtistResource resource)
        {
            if (resource == null) return null;

            return new Core.Music.Artist
            {
                Id = resource.Id,

                Name = resource.Name,
                CleanName = resource.CleanName,
                //AlternateTitles
                //SortTitle = resource.SortTitle,
                MBId = resource.MBId,
                TADBId = resource.TADBId,
                DiscogsId = resource.DiscogsId,
                AMId = resource.AllMusicId, //TODO change model and DB to AllMusic instead of AM
                //TotalEpisodeCount
                //TrackCount
                //TrackFileCount
                //SizeOnDisk
                //Status = resource.Status,
                Overview = resource.Overview,
                //NextAiring
                //PreviousAiring
                //Network = resource.Network,
                //AirTime = resource.AirTime,
                Images = resource.Images,
                Members = resource.Members,
                //Albums = resource.Albums.ToModel(),
                //Year = resource.Year,

                Path = resource.Path,
                ProfileId = resource.ProfileId,
                AlbumFolder = resource.AlbumFolder,

                Monitored = resource.Monitored,
                //LastInfoSync = resource.LastInfoSync,
                ForeignArtistId = resource.ForeignArtistId,
                NameSlug = resource.NameSlug,
                
                RootFolderPath = resource.RootFolderPath,
                Genres = resource.Genres,
                Tags = resource.Tags,
                Added = resource.Added,
                AddOptions = resource.AddOptions,
                Ratings = resource.Ratings
            };
        }

        public static Core.Music.Artist ToModel(this ArtistResource resource, Core.Music.Artist artist)
        {
            var updatedArtist = resource.ToModel();

            artist.ApplyChanges(updatedArtist);

            return artist;
        }

        public static List<ArtistResource> ToResource(this IEnumerable<Core.Music.Artist> artist)
        {
            return artist.Select(ToResource).ToList();
        }
    }
}
