using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.NetImport.Trakt.List
{
    public class TraktListImport : TraktImportBase<TraktListSettings>
    {
        public TraktListImport(INetImportRepository netImportRepository,
                               IHttpClient httpClient,
                               IConfigService configService,
                               IParsingService parsingService,
                               Logger logger)
        : base(netImportRepository, httpClient, configService, parsingService, logger)
        {
        }

        public override string Name => "Trakt List";
        public override bool Enabled => true;
        public override bool EnableAuto => false;

        public override INetImportRequestGenerator GetRequestGenerator()
        {
            return new TraktListRequestGenerator()
            {
                Settings = Settings
            };
        }
    }
}
