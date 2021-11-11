using NzbDrone.Core.Configuration;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Config
{
    public class UiConfigResource : RestResource
    {
        //Calendar
        public int FirstDayOfWeek { get; set; }
        public string CalendarWeekColumnHeader { get; set; }

        // Movies
        public MovieRuntimeFormatType MovieRuntimeFormat { get; set; }

        //Dates
        public string ShortDateFormat { get; set; }
        public string LongDateFormat { get; set; }
        public string TimeFormat { get; set; }
        public bool ShowRelativeDates { get; set; }

        public bool EnableColorImpairedMode { get; set; }
        public int MovieInfoLanguage { get; set; }
        public int UILanguage { get; set; }
        public string Theme { get; set; }
    }

    public static class UiConfigResourceMapper
    {
        public static UiConfigResource ToResource(this IConfigFileProvider model, IConfigService configService)
        {
            return new UiConfigResource
            {
                FirstDayOfWeek = configService.FirstDayOfWeek,
                CalendarWeekColumnHeader = configService.CalendarWeekColumnHeader,

                MovieRuntimeFormat = configService.MovieRuntimeFormat,

                ShortDateFormat = configService.ShortDateFormat,
                LongDateFormat = configService.LongDateFormat,
                TimeFormat = configService.TimeFormat,
                ShowRelativeDates = configService.ShowRelativeDates,

                EnableColorImpairedMode = configService.EnableColorImpairedMode,
                MovieInfoLanguage = configService.MovieInfoLanguage,
                UILanguage = configService.UILanguage,
                Theme = model.Theme
            };
        }
    }
}
