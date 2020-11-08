using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Api.MovieFiles;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Movies;
using Radarr.Http.REST;

namespace NzbDrone.Api.Movies
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
        public List<AlternativeTitleResource> AlternativeTitles { get; set; }
        public int? SecondaryYear { get; set; }
        public int SecondaryYearSourceId { get; set; }
        public string SortTitle { get; set; }
        public long? SizeOnDisk { get; set; }
        public MovieStatusType Status { get; set; }
        public string Overview { get; set; }
        public DateTime? InCinemas { get; set; }
        public DateTime? PhysicalRelease { get; set; }
        public string PhysicalReleaseNote { get; set; }
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
        public MovieStatusType MinimumAvailability { get; set; }
        public bool IsAvailable { get; set; }
        public string FolderName { get; set; }

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

        //public List<string> AlternativeTitles { get; set; }
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
        public static MovieResource ToResource(this Core.Movies.Movie model)
        {
            if (model == null)
            {
                return null;
            }

            long size = model.MovieFile?.Size ?? 0;
            bool downloaded = model.MovieFile != null;
            MovieFileResource movieFile = model.MovieFile?.ToResource();

            /*if(model.MovieFile != null)
            {
                model.MovieFile.LazyLoad();
            }

            if (model.MovieFile != null && model.MovieFile.IsLoaded && model.MovieFile.Value != null)
            {
                size = model.MovieFile.Value.Size;
                downloaded = true;
                movieFile = model.MovieFile.Value.ToResource();
            }*/

            //model.AlternativeTitles.LazyLoad();
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
                SizeOnDisk = size,
                Status = model.Status,
                Overview = model.Overview,

                //NextAiring
                //PreviousAiring
                Images = model.Images,

                Year = model.Year,
                SecondaryYear = model.SecondaryYear,

                Path = model.Path,
                ProfileId = model.ProfileId,

                Monitored = model.Monitored,
                MinimumAvailability = model.MinimumAvailability,

                IsAvailable = model.IsAvailable(),
                FolderName = model.FolderName(),

                //SizeOnDisk = size,
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
                AlternativeTitles = model.AlternativeTitles.ToResource(),
                Ratings = model.Ratings,
                MovieFile = movieFile,
                YouTubeTrailerId = model.YouTubeTrailerId,
                Studio = model.Studio
            };
        }

        public static Core.Movies.Movie ToModel(this MovieResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new Core.Movies.Movie
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
                SecondaryYear = resource.SecondaryYear,

                Path = resource.Path,
                ProfileId = resource.ProfileId,

                Monitored = resource.Monitored,
                MinimumAvailability = resource.MinimumAvailability,

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

                //AlternativeTitles = resource.AlternativeTitles,
                Ratings = resource.Ratings,
                YouTubeTrailerId = resource.YouTubeTrailerId,
                Studio = resource.Studio
            };
        }

        public static Core.Movies.Movie ToModel(this MovieResource resource, Core.Movies.Movie movie)
        {
            var updatedmovie = resource.ToModel();

            movie.ApplyChanges(updatedmovie);

            return movie;
        }

        public static List<MovieResource> ToResource(this IEnumerable<Core.Movies.Movie> movies)
        {
            return movies.Select(ToResource).ToList();
        }
    }
}
