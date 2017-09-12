using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Music;
using Lidarr.Api.V3.Albums;
using Lidarr.Http.REST;

namespace Lidarr.Api.V3.Artist
{
    public class ArtistResource : RestResource
    {
        //Todo: Sorters should be done completely on the client
        //Todo: Is there an easy way to keep IgnoreArticlesWhenSorting in sync between, Series, History, Missing?
        //Todo: We should get the entire Profile instead of ID and Name separately

        ////View Only
        //public string Title { get; set; }
        ////public List<AlternateTitleResource> AlternateTitles { get; set; }
        //public string SortTitle { get; set; }

        //public int SeasonCount
        //{
        //    get
        //    {
        //        if (Seasons == null) return 0;

        //        return Seasons.Where(s => s.SeasonNumber > 0).Count();
        //    }
        //}

        //public int? TotalEpisodeCount { get; set; }
        //public int? EpisodeCount { get; set; }
        //public int? EpisodeFileCount { get; set; }
        //public long? SizeOnDisk { get; set; }

        //// V3: replace with Ended

        public ArtistStatusType Status { get; set; }

        public bool Ended => Status == ArtistStatusType.Ended;

        //public string ProfileName { get; set; }
        //public string Overview { get; set; }
        //public DateTime? NextAiring { get; set; }
        //public DateTime? PreviousAiring { get; set; }
        //public string Network { get; set; }
        //public string AirTime { get; set; }
        //public List<MediaCover> Images { get; set; }

        //public string RemotePoster { get; set; }
        //public int Year { get; set; }

        ////View & Edit
        //public string Path { get; set; }
        //public int QualityProfileId { get; set; }
        //public int LanguageProfileId { get; set; }

        ////Editing Only
        //public bool SeasonFolder { get; set; }
        //public bool Monitored { get; set; }

        //public bool UseSceneNumbering { get; set; }
        //public int Runtime { get; set; }
        //public int TvdbId { get; set; }
        //public int TvRageId { get; set; }
        //public int TvMazeId { get; set; }
        //public DateTime? FirstAired { get; set; }
        public DateTime? LastInfoSync { get; set; }
        ////public SeriesTypes SeriesType { get; set; }
        //public string CleanTitle { get; set; }
        //public string ImdbId { get; set; }
        //public string TitleSlug { get; set; }
        //public string RootFolderPath { get; set; }
        //public string Certification { get; set; }
        //public List<string> Genres { get; set; }
        //public HashSet<int> Tags { get; set; }
        //public DateTime Added { get; set; }
        //public AddSeriesOptions AddOptions { get; set; }
        //public Ratings Ratings { get; set; }

        public string ArtistName { get; set; }
        public string ForeignArtistId { get; set; }
        public string MBId { get; set; }
        public int TADBId { get; set; }
        public int DiscogsId { get; set; }
        public string AllMusicId { get; set; }
        public string Overview { get; set; }

        public int? AlbumCount { get; set; }
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
        public int QualityProfileId { get; set; }
        public int LanguageProfileId { get; set; }

        //Editing Only
        public bool AlbumFolder { get; set; }
        public bool Monitored { get; set; }

        public string RootFolderPath { get; set; }
        //public string Certification { get; set; }
        public List<string> Genres { get; set; }
        public string CleanName { get; set; }
        public string SortName { get; set; }
        public HashSet<int> Tags { get; set; }
        public DateTime Added { get; set; }
        public AddArtistOptions AddOptions { get; set; }
        public Ratings Ratings { get; set; }
        public string NameSlug { get; set; }

        //TODO: Add series statistics as a property of the series (instead of individual properties)
    }

    public static class SeriesResourceMapper
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

                       //TotalEpisodeCount
                       //EpisodeCount
                       //EpisodeFileCount
                       //SizeOnDisk
                       Status = model.Status,
                       Overview = model.Overview,
                       //NextAiring
                       //PreviousAiring
                       //Network = model.Network,
                       //AirTime = model.AirTime,
                       Images = model.Images,

                       Albums = model.Albums.ToResource(),
                       //Year = model.Year,

                       Path = model.Path,
                       QualityProfileId = model.ProfileId,
                       LanguageProfileId = model.LanguageProfileId,

                       AlbumFolder = model.AlbumFolder,
                       Monitored = model.Monitored,

                       //UseSceneNumbering = model.UseSceneNumbering,
                       //Runtime = model.Runtime,
                       //TvdbId = model.TvdbId,
                       //TvRageId = model.TvRageId,
                       //TvMazeId = model.TvMazeId,
                       //FirstAired = model.FirstAired,
                       LastInfoSync = model.LastInfoSync,
                       //SeriesType = model.SeriesType,
                       CleanName = model.CleanName,
                       ForeignArtistId = model.ForeignArtistId,
                       NameSlug = model.NameSlug,
                       RootFolderPath = model.RootFolderPath,
                       //Certification = model.Certification,
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

                       //TotalEpisodeCount
                       //EpisodeCount
                       //EpisodeFileCount
                       //SizeOnDisk
                       Status = resource.Status,
                       Overview = resource.Overview,
                       //NextAiring
                       //PreviousAiring
                      // Network = resource.Network,
                       //AirTime = resource.AirTime,
                       Images = resource.Images,

                       //Albums = resource.Albums.ToModel(),
                       //Year = resource.Year,

                       Path = resource.Path,
                       ProfileId = resource.QualityProfileId,
                       LanguageProfileId = resource.LanguageProfileId,

                       AlbumFolder = resource.AlbumFolder,
                       Monitored = resource.Monitored,

                       //UseSceneNumbering = resource.UseSceneNumbering,
                       //Runtime = resource.Runtime,
                       //TvdbId = resource.TvdbId,
                       //TvRageId = resource.TvRageId,
                       //TvMazeId = resource.TvMazeId,
                       //FirstAired = resource.FirstAired,
                       LastInfoSync = resource.LastInfoSync,
                       //SeriesType = resource.SeriesType,
                       CleanName = resource.CleanName,
                       ForeignArtistId = resource.ForeignArtistId,
                       NameSlug = resource.NameSlug,
                       RootFolderPath = resource.RootFolderPath,
                       //Certification = resource.Certification,
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
