namespace NzbDrone.Core.Configuration
{
    public interface IUIConfigService
    {
        int FirstDayOfWeek { get; set; }
        string CalendarWeekColumnHeader { get; set; }
        MovieRuntimeFormatType MovieRuntimeFormat { get; set; }
        string ShortDateFormat { get; set; }
        string LongDateFormat { get; set; }
        string TimeFormat { get; set; }
        bool ShowRelativeDates { get; set; }
        bool EnableColorImpairedMode { get; set; }
        int MovieInfoLanguage { get; set; }
        int UILanguage { get; set; }
    }
}
