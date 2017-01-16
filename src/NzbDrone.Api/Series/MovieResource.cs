using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Api.REST;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Tv;
using NzbDrone.Api.Series;

namespace NzbDrone.Api.Movie
{
    public class MovieResource : RestResource
    {
        public MovieResource()
        {
            Monitored = true;
        }

        //Todo: Sorters should be done completely on the client
        //Todo: Is there an easy way to keep IgnoreArticlesWhenSorting in sync between, Series, History, Missing?
        //Todo: We should get the entire Profile instead of ID and Name separately
         
        //View Only
        public string Title { get; set; }
        public List<AlternateTitleResource> AlternateTitles { get; set; }
        public string SortTitle { get; set; }
        public long? SizeOnDisk { get; set; }
        public MovieStatusType Status { get; set; }
        public string Overview { get; set; }
        public DateTime? InCinemas { get; set; }
        public DateTime? PhysicalRelease { get; set; }
        public List<MediaCover> Images { get; set; }
        public string Website { get; set; }
        public bool Downloaded { get; set; }
        public string RemotePoster { get; set; }
        public int Year { get; set; }
        public bool HasFile { get; set; }
        public string YouTubeTrailerId { get; set; }
        public string Studio { get; set; }

        //View & Edit
        public string Path { get; set; }
        public int ProfileId { get; set; }

        //Editing Only
        public bool Monitored { get; set; }
        public int Runtime { get; set; }
        public DateTime? LastInfoSync { get; set; }
        public string CleanTitle { get; set; }
        public string ImdbId { get; set; }
        public int TmdbId { get; set; }
        public string TitleSlug { get; set; }
        public string RootFolderPath { get; set; }
        public string Certification { get; set; }
        public List<string> Genres { get; set; }
        public HashSet<int> Tags { get; set; }
        public DateTime Added { get; set; }
        public AddMovieOptions AddOptions { get; set; }
        public Ratings Ratings { get; set; }
        public List<string> AlternativeTitles { get; set; }
        public MovieFileResource MovieFile { get; set; }

        //TODO: Add series statistics as a property of the series (instead of individual properties)

        //Used to support legacy consumers
        public int QualityProfileId
        {
            get
            {
                return ProfileId;
            }
            set
            {
                if (value > 0 && ProfileId == 0)
                {
                    ProfileId = value;
                }
            }
        }
    }

    public static class MovieResourceMapper
    {
        public static MovieResource ToResource(this Core.Tv.Movie model)
        {
            if (model == null) return null;


            long size = 0;
            bool downloaded = false;
            MovieFileResource movieFile = null;

            
            if(model.MovieFile != null)
            {
                model.MovieFile.LazyLoad();
            }

            if (model.MovieFile != null && model.MovieFile.IsLoaded && model.MovieFile.Value != null)
            {
                size = model.MovieFile.Value.Size;
                downloaded = true;
                movieFile = model.MovieFile.Value.ToResource();
            }

            return new MovieResource
            {
                Id = model.Id,
                TmdbId = model.TmdbId,
                Title = model.Title,
                //AlternateTitles
                SortTitle = model.SortTitle,
                InCinemas = model.InCinemas,
                PhysicalRelease = model.PhysicalRelease,
                HasFile = model.HasFile,
                Downloaded = downloaded,
                //TotalEpisodeCount
                //EpisodeCount
                //EpisodeFileCount
                //SizeOnDisk
                Status = model.Status,
                Overview = model.Overview,
                //NextAiring
                //PreviousAiring
                Images = model.Images,
                
                Year = model.Year,
                
                Path = model.Path,
                ProfileId = model.ProfileId,
                
                Monitored = model.Monitored,

                SizeOnDisk = size,

                Runtime = model.Runtime,
                LastInfoSync = model.LastInfoSync,
                CleanTitle = model.CleanTitle,
                ImdbId = model.ImdbId,
                TitleSlug = model.TitleSlug,
                RootFolderPath = model.RootFolderPath,
                Certification = model.Certification,
                Website = model.Website,
                Genres = model.Genres,
                Tags = model.Tags,
                Added = model.Added,
                AddOptions = model.AddOptions,
                AlternativeTitles = model.AlternativeTitles,
                Ratings = model.Ratings,
                MovieFile = movieFile,
                YouTubeTrailerId = model.YouTubeTrailerId,
                Studio = model.Studio
            };
        }

        public static Core.Tv.Movie ToModel(this MovieResource resource)
        {
            if (resource == null) return null;

            return new Core.Tv.Movie
            {
                Id = resource.Id,
                TmdbId = resource.TmdbId,

                Title = resource.Title,
                //AlternateTitles
                SortTitle = resource.SortTitle,
                InCinemas = resource.InCinemas,
                PhysicalRelease = resource.PhysicalRelease,
                //TotalEpisodeCount
                //EpisodeCount
                //EpisodeFileCount
                //SizeOnDisk
                Overview = resource.Overview,
                //NextAiring
                //PreviousAiring
                Images = resource.Images,

                Year = resource.Year,

                Path = resource.Path,
                ProfileId = resource.ProfileId,

                Monitored = resource.Monitored,

                Runtime = resource.Runtime,
                LastInfoSync = resource.LastInfoSync,
                CleanTitle = resource.CleanTitle,
                ImdbId = resource.ImdbId,
                TitleSlug = resource.TitleSlug,
                RootFolderPath = resource.RootFolderPath,
                Certification = resource.Certification,
                Website = resource.Website,
                Genres = resource.Genres,
                Tags = resource.Tags,
                Added = resource.Added,
                AddOptions = resource.AddOptions,
                AlternativeTitles = resource.AlternativeTitles,
                Ratings = resource.Ratings,
                YouTubeTrailerId = resource.YouTubeTrailerId,
                Studio = resource.Studio
            };
        }

        public static Core.Tv.Movie ToModel(this MovieResource resource, Core.Tv.Movie movie)
        {
            movie.ImdbId = resource.ImdbId;
            movie.TmdbId = resource.TmdbId;

            movie.Path = resource.Path;
            movie.ProfileId = resource.ProfileId;

            movie.Monitored = resource.Monitored;

            movie.RootFolderPath = resource.RootFolderPath;
            movie.Tags = resource.Tags;
            movie.AddOptions = resource.AddOptions;

            return movie;
        }

        public static List<MovieResource> ToResource(this IEnumerable<Core.Tv.Movie> movies)
        {
            return movies.Select(ToResource).ToList();
        }
    }
}
