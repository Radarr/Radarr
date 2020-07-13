using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.NetImport.StevenLu
{
    public class StevenLuImport : HttpNetImportBase<StevenLuSettings>
    {
        public override string Name => "StevenLu Custom";

        public override NetImportType ListType => NetImportType.Advanced;
        public override bool Enabled => true;
        public override bool EnableAuto => false;

        public StevenLuImport(IHttpClient httpClient,
                              INetImportStatusService netImportStatusService,
                              IConfigService configService,
                              IParsingService parsingService,
                              Logger logger)
            : base(httpClient, netImportStatusService, configService, parsingService, logger)
        {
        }

        public override INetImportRequestGenerator GetRequestGenerator()
        {
            return new StevenLuRequestGenerator() { Settings = Settings };
        }

        public override IParseNetImportResponse GetParser()
        {
            return new StevenLuParser();
        }
    }
}
