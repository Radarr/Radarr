using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Movies;
using Radarr.Api.V3.MovieFiles;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Movies
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
        public List<AlternativeTitleResource> AlternateTitles { get; set; }
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

        //public bool Downloaded { get; set; }
        public string RemotePoster { get; set; }
        public int Year { get; set; }
        public bool HasFile { get; set; }
        public string YouTubeTrailerId { get; set; }
        public string Studio { get; set; }

        //View & Edit
        public string Path { get; set; }
        public int QualityProfileId { get; set; }
        public MoviePathState PathState { get; set; }

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
        public string Folder { get; set; }
        public string Certification { get; set; }
        public List<string> Genres { get; set; }
        public HashSet<int> Tags { get; set; }
        public DateTime Added { get; set; }
        public AddMovieOptions AddOptions { get; set; }
        public Ratings Ratings { get; set; }
        public MovieFileResource MovieFile { get; set; }
    }

    public static class MovieResourceMapper
    {
        public static MovieResource ToResource(this Movie model)
        {
            if (model == null)
            {
                return null;
            }

            long size = model.MovieFile?.Size ?? 0;
            MovieFileResource movieFile = model.MovieFile?.ToResource(model);

            return new MovieResource
            {
                Id = model.Id,
                TmdbId = model.TmdbId,
                Title = model.Title,
                SortTitle = model.SortTitle,
                InCinemas = model.InCinemas,
                PhysicalRelease = model.PhysicalRelease,
                PhysicalReleaseNote = model.PhysicalReleaseNote,
                HasFile = model.HasFile,

                SizeOnDisk = size,
                Status = model.Status,
                Overview = model.Overview,

                Images = model.Images,

                Year = model.Year,
                SecondaryYear = model.SecondaryYear,
                SecondaryYearSourceId = model.SecondaryYearSourceId,

                Path = model.Path,
                QualityProfileId = model.ProfileId,
                PathState = model.PathState,

                Monitored = model.Monitored,
                MinimumAvailability = model.MinimumAvailability,

                IsAvailable = model.IsAvailable(),
                FolderName = model.FolderName(),

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
                AlternateTitles = model.AlternativeTitles.ToResource(),
                Ratings = model.Ratings,
                MovieFile = movieFile,
                YouTubeTrailerId = model.YouTubeTrailerId,
                Studio = model.Studio
            };
        }

        public static MovieResource ToResource(this Movie model, IUpgradableSpecification upgradableSpecification)
        {
            if (model == null)
            {
                return null;
            }

            long size = model.MovieFile?.Size ?? 0;
            MovieFileResource movieFile = model.MovieFile?.ToResource(model, upgradableSpecification);

            return new MovieResource
            {
                Id = model.Id,
                TmdbId = model.TmdbId,
                Title = model.Title,
                SortTitle = model.SortTitle,
                InCinemas = model.InCinemas,
                PhysicalRelease = model.PhysicalRelease,
                PhysicalReleaseNote = model.PhysicalReleaseNote,
                HasFile = model.HasFile,

                SizeOnDisk = size,
                Status = model.Status,
                Overview = model.Overview,

                Images = model.Images,

                Year = model.Year,
                SecondaryYear = model.SecondaryYear,
                SecondaryYearSourceId = model.SecondaryYearSourceId,

                Path = model.Path,
                QualityProfileId = model.ProfileId,
                PathState = model.PathState,

                Monitored = model.Monitored,
                MinimumAvailability = model.MinimumAvailability,

                IsAvailable = model.IsAvailable(),
                FolderName = model.FolderName(),

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
                AlternateTitles = model.AlternativeTitles.ToResource(),
                Ratings = model.Ratings,
                MovieFile = movieFile,
                YouTubeTrailerId = model.YouTubeTrailerId,
                Studio = model.Studio
            };
        }

        public static Movie ToModel(this MovieResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new Movie
            {
                Id = resource.Id,
                TmdbId = resource.TmdbId,

                Title = resource.Title,
                SortTitle = resource.SortTitle,
                InCinemas = resource.InCinemas,
                PhysicalRelease = resource.PhysicalRelease,
                PhysicalReleaseNote = resource.PhysicalReleaseNote,

                Overview = resource.Overview,

                Images = resource.Images,

                Year = resource.Year,
                SecondaryYear = resource.SecondaryYear,
                SecondaryYearSourceId = resource.SecondaryYearSourceId,

                Path = resource.Path,
                ProfileId = resource.QualityProfileId,
                PathState = resource.PathState,

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
                Ratings = resource.Ratings,
                YouTubeTrailerId = resource.YouTubeTrailerId,
                Studio = resource.Studio
            };
        }

        public static Movie ToModel(this MovieResource resource, Movie movie)
        {
            var updatedmovie = resource.ToModel();

            movie.ApplyChanges(updatedmovie);

            return movie;
        }

        public static List<MovieResource> ToResource(this IEnumerable<Movie> movies)
        {
            return movies.Select(ToResource).ToList();
        }

        public static List<MovieResource> ToResource(this IEnumerable<Movie> movies, IUpgradableSpecification upgradableSpecification)
        {
            return movies.ToList().ConvertAll(f => f.ToResource(upgradableSpecification));
        }

        public static List<Movie> ToModel(this IEnumerable<MovieResource> resources)
        {
            return resources.Select(ToModel).ToList();
        }
    }
}
