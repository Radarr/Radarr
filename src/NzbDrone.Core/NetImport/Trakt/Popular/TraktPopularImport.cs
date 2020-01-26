using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.NetImport.Trakt.Popular
{
    public class TraktPopularImport : TraktImportBase<TraktPopularSettings>
    {
        public TraktPopularImport(INetImportRepository netImportRepository,
                   IHttpClient httpClient,
                   IConfigService configService,
                   IParsingService parsingService,
                   Logger logger)
        : base(netImportRepository, httpClient, configService, parsingService, logger)
        {
        }

        public override string Name => "Trakt Popular List";
        public override bool Enabled => true;
        public override bool EnableAuto => false;

        public override IParseNetImportResponse GetParser()
        {
            return new TraktPopularParser(Settings);
        }

        public override INetImportRequestGenerator GetRequestGenerator()
        {
            return new TraktPopularRequestGenerator()
            {
                Settings = Settings
            };
        }
    }
}
