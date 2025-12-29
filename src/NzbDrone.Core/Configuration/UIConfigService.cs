namespace NzbDrone.Core.Configuration
{
    public class UIConfigService : IUIConfigService
    {
        private readonly IConfigService _configService;

        public UIConfigService(IConfigService configService)
        {
            _configService = configService;
        }

        public int FirstDayOfWeek
        {
            get => _configService.FirstDayOfWeek;
            set => _configService.FirstDayOfWeek = value;
        }

        public string CalendarWeekColumnHeader
        {
            get => _configService.CalendarWeekColumnHeader;
            set => _configService.CalendarWeekColumnHeader = value;
        }

        public MovieRuntimeFormatType MovieRuntimeFormat
        {
            get => _configService.MovieRuntimeFormat;
            set => _configService.MovieRuntimeFormat = value;
        }

        public string ShortDateFormat
        {
            get => _configService.ShortDateFormat;
            set => _configService.ShortDateFormat = value;
        }

        public string LongDateFormat
        {
            get => _configService.LongDateFormat;
            set => _configService.LongDateFormat = value;
        }

        public string TimeFormat
        {
            get => _configService.TimeFormat;
            set => _configService.TimeFormat = value;
        }

        public bool ShowRelativeDates
        {
            get => _configService.ShowRelativeDates;
            set => _configService.ShowRelativeDates = value;
        }

        public bool EnableColorImpairedMode
        {
            get => _configService.EnableColorImpairedMode;
            set => _configService.EnableColorImpairedMode = value;
        }

        public int MovieInfoLanguage
        {
            get => _configService.MovieInfoLanguage;
            set => _configService.MovieInfoLanguage = value;
        }

        public int UILanguage
        {
            get => _configService.UILanguage;
            set => _configService.UILanguage = value;
        }
    }
}
