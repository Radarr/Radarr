using NzbDrone.Api.REST;
using NzbDrone.Api.Series;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Tv;
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
        public string ArtistName { get; set; }
        public int ItunesId { get; set; }
        //public List<AlternateTitleResource> AlternateTitles { get; set; }
        //public string SortTitle { get; set; }

        public int AlbumCount
        {
            get
            {
                if (Albums == null) return 0;

                return Albums.Where(s => s.AlbumId > 0).Count(); // TODO: CHeck this condition
            }
        }

        public int? TotalTrackCount { get; set; }
        public int? TrackCount { get; set; }
        public int? TrackFileCount { get; set; }
        public long? SizeOnDisk { get; set; }
        //public SeriesStatusType Status { get; set; }
        
        public List<MediaCover> Images { get; set; }

        public string RemotePoster { get; set; }
        public List<AlbumResource> Albums { get; set; }
        

        //View & Edit
        public string Path { get; set; }
        public int ProfileId { get; set; }

        //Editing Only
        public bool ArtistFolder { get; set; }
        public bool Monitored { get; set; }

        public string RootFolderPath { get; set; }
        public string Certification { get; set; }
        public List<string> Genres { get; set; }
        public HashSet<int> Tags { get; set; }
        public DateTime Added { get; set; }
        public AddSeriesOptions AddOptions { get; set; }
        public Ratings Ratings { get; set; }
        public string ArtistSlug { get; internal set; }
    }

    public static class ArtistResourceMapper
    {
        public static ArtistResource ToResource(this Core.Music.Artist model)
        {
            if (model == null) return null;

            return new ArtistResource
            {
                Id = model.Id,

                ArtistName = model.ArtistName,
                //AlternateTitles
                //SortTitle = resource.SortTitle,

                //TotalEpisodeCount
                //EpisodeCount
                //EpisodeFileCount
                //SizeOnDisk
                //Status = resource.Status,
                //Overview = resource.Overview,
                //NextAiring
                //PreviousAiring
                //Network = resource.Network,
                //AirTime = resource.AirTime,
                Images = model.Images,

                Albums = model.Albums.ToResource(),
                //Year = resource.Year,

                Path = model.Path,
                ProfileId = model.ProfileId,

                ArtistFolder = model.ArtistFolder,
                Monitored = model.Monitored,

                //UseSceneNumbering = resource.UseSceneNumbering,
                //Runtime = resource.Runtime,
                //TvdbId = resource.TvdbId,
                //TvRageId = resource.TvRageId,
                //TvMazeId = resource.TvMazeId,
                //FirstAired = resource.FirstAired,
                //LastInfoSync = resource.LastInfoSync,
                //SeriesType = resource.SeriesType,
                ItunesId = model.ItunesId,
                ArtistSlug = model.ArtistSlug,

                RootFolderPath = model.RootFolderPath,
                Genres = model.Genres,
                Tags = model.Tags,
                Added = model.Added,
                AddOptions = model.AddOptions,
                //Ratings = resource.Ratings
            };
        }

        public static Core.Music.Artist ToModel(this ArtistResource resource)
        {
            if (resource == null) return null;

            return new Core.Music.Artist
            {
                Id = resource.Id,

                ArtistName = resource.ArtistName,
                //AlternateTitles
                //SortTitle = resource.SortTitle,

                //TotalEpisodeCount
                //EpisodeCount
                //EpisodeFileCount
                //SizeOnDisk
                //Status = resource.Status,
                //Overview = resource.Overview,
                //NextAiring
                //PreviousAiring
                //Network = resource.Network,
                //AirTime = resource.AirTime,
                Images = resource.Images,

                Albums = resource.Albums.ToModel(),
                //Year = resource.Year,

                Path = resource.Path,
                ProfileId = resource.ProfileId,

                ArtistFolder = resource.ArtistFolder,
                Monitored = resource.Monitored,

                //UseSceneNumbering = resource.UseSceneNumbering,
                //Runtime = resource.Runtime,
                //TvdbId = resource.TvdbId,
                //TvRageId = resource.TvRageId,
                //TvMazeId = resource.TvMazeId,
                //FirstAired = resource.FirstAired,
                //LastInfoSync = resource.LastInfoSync,
                //SeriesType = resource.SeriesType,
                ItunesId = resource.ItunesId,
                ArtistSlug = resource.ArtistSlug,
                
                RootFolderPath = resource.RootFolderPath,
                Genres = resource.Genres,
                Tags = resource.Tags,
                Added = resource.Added,
                AddOptions = resource.AddOptions,
                //Ratings = resource.Ratings
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
