using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Configuration;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Config
{
    public class UiConfigResource : RestResource
    {
        // Calendar
        public int FirstDayOfWeek { get; set; }
        public string CalendarWeekColumnHeader { get; set; }

        // Movies
        public MovieRuntimeFormatType MovieRuntimeFormat { get; set; }

        // Dates
        public string ShortDateFormat { get; set; }
        public string LongDateFormat { get; set; }
        public string TimeFormat { get; set; }
        public bool ShowRelativeDates { get; set; }

        public bool EnableColorImpairedMode { get; set; }
        public int MovieInfoLanguage { get; set; }
        public int UILanguage { get; set; }
        public string Theme { get; set; }

        // Poster Replacement
        public bool PosterReplacementEnabled { get; set; }
        public List<string> PosterReplacementGenres { get; set; }
        public List<int> PosterReplacementTags { get; set; }
        public string PosterReplacementBackgroundColor { get; set; }
        public string PosterReplacementTextColor { get; set; }
    }

    public static class UiConfigResourceMapper
    {
        public static UiConfigResource ToResource(IConfigFileProvider config, IConfigService model)
        {
            return new UiConfigResource
            {
                FirstDayOfWeek = model.FirstDayOfWeek,
                CalendarWeekColumnHeader = model.CalendarWeekColumnHeader,

                MovieRuntimeFormat = model.MovieRuntimeFormat,

                ShortDateFormat = model.ShortDateFormat,
                LongDateFormat = model.LongDateFormat,
                TimeFormat = model.TimeFormat,
                ShowRelativeDates = model.ShowRelativeDates,

                EnableColorImpairedMode = model.EnableColorImpairedMode,
                MovieInfoLanguage = model.MovieInfoLanguage,
                UILanguage = model.UILanguage,
                Theme = config.Theme,

                PosterReplacementEnabled = model.PosterReplacementEnabled,
                PosterReplacementGenres = model.PosterReplacementGenres.ToList(),
                PosterReplacementTags = model.PosterReplacementTags.ToList(),
                PosterReplacementBackgroundColor = model.PosterReplacementBackgroundColor,
                PosterReplacementTextColor = model.PosterReplacementTextColor
            };
        }
    }
}
