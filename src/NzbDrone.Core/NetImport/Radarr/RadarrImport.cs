using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.NetImport.Radarr
{
    public class RadarrImport : HttpNetImportBase<RadarrSettings>
    {
        public override string Name => "Radarr";
        public override bool Enabled => true;
        public override bool EnableAuto => false;

        public override NetImportType ListType => NetImportType.Other;

        public RadarrImport(IHttpClient httpClient,
                            IConfigService configService,
                            IParsingService parsingService,
                            Logger logger)
            : base(httpClient, configService, parsingService, logger)
        {
        }

        public override INetImportRequestGenerator GetRequestGenerator()
        {
            return new RadarrRequestGenerator()
            {
                Settings = Settings,
                Logger = _logger,
                HttpClient = _httpClient
            };
        }

        public override IParseNetImportResponse GetParser()
        {
            return new RadarrParser(Settings);
        }
    }
}
