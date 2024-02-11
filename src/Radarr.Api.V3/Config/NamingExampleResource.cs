using NzbDrone.Core.Organizer;

namespace Radarr.Api.V3.Config
{
    public class NamingExampleResource
    {
        public string MovieExample { get; set; }
        public string MovieFolderExample { get; set; }
    }

    public static class NamingConfigResourceMapper
    {
        public static NamingConfigResource ToResource(this NamingConfig model)
        {
            return new NamingConfigResource
            {
                Id = model.Id,

                RenameMovies = model.RenameMovies,
                ReplaceIllegalCharacters = model.ReplaceIllegalCharacters,
                ColonReplacementFormat = model.ColonReplacementFormat,
                StandardMovieFormat = model.StandardMovieFormat,
                MovieFolderFormat = model.MovieFolderFormat
            };
        }

        public static NamingConfig ToModel(this NamingConfigResource resource)
        {
            return new NamingConfig
            {
                Id = resource.Id,

                RenameMovies = resource.RenameMovies,
                ReplaceIllegalCharacters = resource.ReplaceIllegalCharacters,
                ColonReplacementFormat = resource.ColonReplacementFormat,
                StandardMovieFormat = resource.StandardMovieFormat,
                MovieFolderFormat = resource.MovieFolderFormat,
            };
        }
    }
}
