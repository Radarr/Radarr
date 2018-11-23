using NzbDrone.Core.Organizer;

namespace Radarr.Api.V2.Config
{
    public class NamingExampleResource
    {
        //public string SingleEpisodeExample { get; set; }
        //public string MultiEpisodeExample { get; set; }
        //public string DailyEpisodeExample { get; set; }
        //public string AnimeEpisodeExample { get; set; }
        //public string AnimeMultiEpisodeExample { get; set; }
        //public string SeriesFolderExample { get; set; }
        //public string SeasonFolderExample { get; set; }

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

                RenameEpisodes = model.RenameEpisodes,
                ReplaceIllegalCharacters = model.ReplaceIllegalCharacters,
                MultiEpisodeStyle = model.MultiEpisodeStyle,
                ColonReplacementFormat = model.ColonReplacementFormat,
                StandardMovieFormat = model.StandardMovieFormat,
                MovieFolderFormat = model.MovieFolderFormat,
                //IncludeSeriesTitle
                //IncludeEpisodeTitle
                //IncludeQuality
                //ReplaceSpaces
                //Separator
                //NumberStyle
            };
        }

        public static void AddToResource(this BasicNamingConfig basicNamingConfig, NamingConfigResource resource)
        {
            resource.IncludeSeriesTitle = basicNamingConfig.IncludeSeriesTitle;
            resource.IncludeEpisodeTitle = basicNamingConfig.IncludeEpisodeTitle;
            resource.IncludeQuality = basicNamingConfig.IncludeQuality;
            resource.ReplaceSpaces = basicNamingConfig.ReplaceSpaces;
            resource.Separator = basicNamingConfig.Separator;
            resource.NumberStyle = basicNamingConfig.NumberStyle;
        }

        public static NamingConfig ToModel(this NamingConfigResource resource)
        {
            return new NamingConfig
            {
                Id = resource.Id,

                RenameEpisodes = resource.RenameEpisodes,
                ReplaceIllegalCharacters = resource.ReplaceIllegalCharacters,
                MultiEpisodeStyle = resource.MultiEpisodeStyle,
                ColonReplacementFormat = resource.ColonReplacementFormat,
                StandardMovieFormat = resource.StandardMovieFormat,
                MovieFolderFormat = resource.MovieFolderFormat,
            };
        }
    }
}
