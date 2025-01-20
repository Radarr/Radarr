using NzbDrone.Core.Movies.AlternativeTitles;
using Radarr.Http.REST;

namespace Radarr.Api.V4.Movies;

public class AlternativeTitleResource : RestResource
{
    public SourceType SourceType { get; set; }
    public int MovieMetadataId { get; set; }
    public string? Title { get; set; }
    public string? CleanTitle { get; set; }
}

public static class AlternativeTitleResourceMapper
{
    public static AlternativeTitleResource ToResource(this AlternativeTitle model)
    {
        return new AlternativeTitleResource
        {
            Id = model.Id,
            SourceType = model.SourceType,
            MovieMetadataId = model.MovieMetadataId,
            Title = model.Title
        };
    }

    public static AlternativeTitle ToModel(this AlternativeTitleResource resource)
    {
        return new AlternativeTitle
        {
            Id = resource.Id,
            SourceType = resource.SourceType,
            MovieMetadataId = resource.MovieMetadataId,
            Title = resource.Title
        };
    }

    public static List<AlternativeTitleResource> ToResource(this IEnumerable<AlternativeTitle> movies)
    {
        return movies.Select(ToResource).ToList();
    }
}
