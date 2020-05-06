using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.NetImport.Trakt.User
{
    public class TraktUserImport : TraktImportBase<TraktUserSettings>
    {
        public TraktUserImport(INetImportRepository netImportRepository,
                               IHttpClient httpClient,
                               IConfigService configService,
                               IParsingService parsingService,
                               Logger logger)
        : base(netImportRepository, httpClient, configService, parsingService, logger)
        {
        }

        public override string Name => "Trakt User";
        public override bool Enabled => true;
        public override bool EnableAuto => false;

        public override INetImportRequestGenerator GetRequestGenerator()
        {
            return new TraktUserRequestGenerator()
            {
                Settings = Settings
            };
        }
    }
}
