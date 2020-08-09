using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Notifications.Trakt;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.NetImport.Trakt.List
{
    public class TraktListImport : TraktImportBase<TraktListSettings>
    {
        public TraktListImport(INetImportRepository netImportRepository,
                               ITraktProxy traktProxy,
                               IHttpClient httpClient,
                               INetImportStatusService netImportStatusService,
                               IConfigService configService,
                               IParsingService parsingService,
                               Logger logger)
        : base(netImportRepository, traktProxy, httpClient, netImportStatusService, configService, parsingService, logger)
        {
        }

        public override string Name => "Trakt List";
        public override bool Enabled => true;
        public override bool EnableAuto => false;

        public override INetImportRequestGenerator GetRequestGenerator()
        {
            return new TraktListRequestGenerator(_traktProxy)
            {
                Settings = Settings
            };
        }
    }
}
