using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.Filmweb.User
{
    public class FilmwebUserImport : FilmwebImportBase<FilmwebUserSettings>
    {
        public FilmwebUserImport(IHttpClient httpClient,
                                IImportListStatusService importListStatusService,
                                IConfigService configService,
                                IParsingService parsingService,
                                Logger logger)
            : base(httpClient, importListStatusService, configService, parsingService, logger)
        {
        }

        public override string Name => "Filmweb Watchlist";
        public override bool Enabled => true;
        public override bool EnableAuto => false;

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new FilmwebUserRequestGenerator
            {
                Settings = Settings,
                HttpClient = _httpClient,
                Logger = _logger
            };
        }
    }
}
