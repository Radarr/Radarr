using System.Text.Json.Serialization;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Translations;
using NzbDrone.Core.Parser;
using Radarr.Http.REST;
using Swashbuckle.AspNetCore.Annotations;

namespace Radarr.Api.V4.Movies;

public class MovieResource : RestResource
{
    public string? Title { get; set; }
    public string? OriginalTitle { get; set; }
    public Language? OriginalLanguage { get; set; }
    public List<AlternativeTitleResource>? AlternateTitles { get; set; }
    public string? SortTitle { get; set; }
    public MovieStatusType Status { get; set; }
    public string? Overview { get; set; }
    public DateTime? InCinemas { get; set; }
    public DateTime? PhysicalRelease { get; set; }
    public DateTime? DigitalRelease { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public List<MediaCover>? Images { get; set; }
    public string? Website { get; set; }
    public string? RemotePoster { get; set; }
    public int Year { get; set; }
    public int? SecondaryYear { get; set; }
    public string? YouTubeTrailerId { get; set; }
    public string? Studio { get; set; }
    public string? Path { get; set; }
    public int QualityProfileId { get; set; }
    public bool Monitored { get; set; } = true;
    public MovieStatusType MinimumAvailability { get; set; } = MovieStatusType.Released;
    public bool IsAvailable { get; set; }
    public int Runtime { get; set; }
    public string? CleanTitle { get; set; }
    public string? ImdbId { get; set; }
    public int TmdbId { get; set; }
    public string? TitleSlug { get; set; }
    public string? RootFolderPath { get; set; }
    public string? Folder { get; set; }
    public string? Certification { get; set; }
    public MovieCollectionResource? Collection { get; set; }
    public List<string>? Genres { get; set; }
    public HashSet<int>? Tags { get; set; }
    public AddMovieOptions? AddOptions { get; set; }
    public Ratings? Ratings { get; set; }
    public float Popularity { get; set; }
    public MovieStatisticsResource? Statistics { get; set; }
    public DateTime? LastSearchTime { get; set; }
    public DateTime Added { get; set; }

    // Hiding this so people don't think its usable (only used to set the initial state)
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [SwaggerIgnore]
    public bool Grabbed { get; set; }

    // Hiding this so people don't think its usable (only used for searches)
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [SwaggerIgnore]
    public bool IsExcluded { get; set; }
}

public static class MovieResourceMapper
{
    public static MovieResource ToResource(this Movie model, int availabilityDelay, MovieTranslation? movieTranslation)
    {
        var translatedTitle = movieTranslation?.Title ?? model.Title;
        var translatedOverview = movieTranslation?.Overview ?? model.MovieMetadata.Value.Overview;

        var collection = model.MovieMetadata.Value.CollectionTmdbId > 0 ? new MovieCollectionResource { TmdbId = model.MovieMetadata.Value.CollectionTmdbId, Title = model.MovieMetadata.Value.CollectionTitle } : null;

        return new MovieResource
        {
            Id = model.Id,
            TmdbId = model.TmdbId,
            Title = translatedTitle,
            OriginalTitle = model.MovieMetadata.Value.OriginalTitle,
            OriginalLanguage = model.MovieMetadata.Value.OriginalLanguage,
            SortTitle = translatedTitle.NormalizeTitle(),
            InCinemas = model.MovieMetadata.Value.InCinemas,
            PhysicalRelease = model.MovieMetadata.Value.PhysicalRelease,
            DigitalRelease = model.MovieMetadata.Value.DigitalRelease,
            ReleaseDate = model.GetReleaseDate(),
            Status = model.MovieMetadata.Value.Status,
            Overview = translatedOverview,
            Images = model.MovieMetadata.Value.Images.JsonClone(),
            Year = model.Year,
            SecondaryYear = model.MovieMetadata.Value.SecondaryYear,
            Path = model.Path,
            QualityProfileId = model.QualityProfileId,
            Monitored = model.Monitored,
            MinimumAvailability = model.MinimumAvailability,
            IsAvailable = model.IsAvailable(availabilityDelay),
            Runtime = model.MovieMetadata.Value.Runtime,
            CleanTitle = model.MovieMetadata.Value.CleanTitle,
            ImdbId = model.ImdbId,
            TitleSlug = model.MovieMetadata.Value.TmdbId.ToString(),
            RootFolderPath = model.RootFolderPath,
            Certification = model.MovieMetadata.Value.Certification,
            Website = model.MovieMetadata.Value.Website,
            Genres = model.MovieMetadata.Value.Genres,
            Tags = model.Tags,
            Added = model.Added,
            AddOptions = model.AddOptions,
            AlternateTitles = model.MovieMetadata.Value.AlternativeTitles.ToResource(),
            Ratings = model.MovieMetadata.Value.Ratings,
            YouTubeTrailerId = model.MovieMetadata.Value.YouTubeTrailerId,
            Studio = model.MovieMetadata.Value.Studio,
            Collection = collection,
            Popularity = model.MovieMetadata.Value.Popularity,
            LastSearchTime = model.LastSearchTime,
        };
    }

    public static Movie ToModel(this MovieResource resource)
    {
        return new Movie
        {
            Id = resource.Id,

            MovieMetadata = new MovieMetadata
            {
                TmdbId = resource.TmdbId,
                Title = resource.Title,
                Genres = resource.Genres,
                Images = resource.Images,
                OriginalTitle = resource.OriginalTitle,
                SortTitle = resource.SortTitle,
                InCinemas = resource.InCinemas,
                PhysicalRelease = resource.PhysicalRelease,
                Year = resource.Year,
                SecondaryYear = resource.SecondaryYear,
                Overview = resource.Overview,
                Certification = resource.Certification,
                Website = resource.Website,
                Ratings = resource.Ratings,
                YouTubeTrailerId = resource.YouTubeTrailerId,
                Studio = resource.Studio,
                Runtime = resource.Runtime,
                CleanTitle = resource.CleanTitle,
                ImdbId = resource.ImdbId,
            },

            Path = resource.Path,
            QualityProfileId = resource.QualityProfileId,
            Monitored = resource.Monitored,
            MinimumAvailability = resource.MinimumAvailability,
            RootFolderPath = resource.RootFolderPath,
            Tags = resource.Tags ?? new HashSet<int>(),
            Added = resource.Added,
            AddOptions = resource.AddOptions
        };
    }

    public static Movie ToModel(this MovieResource resource, Movie movie)
    {
        var updatedMovie = resource.ToModel();

        movie.ApplyChanges(updatedMovie);

        return movie;
    }

    public static List<MovieResource> ToResource(this IEnumerable<Movie> movies, int availabilityDelay)
    {
        return movies.Select(movie => ToResource(movie, availabilityDelay, null)).ToList();
    }

    public static List<Movie> ToModel(this IEnumerable<MovieResource> resources)
    {
        return resources.Select(ToModel).ToList();
    }
}
