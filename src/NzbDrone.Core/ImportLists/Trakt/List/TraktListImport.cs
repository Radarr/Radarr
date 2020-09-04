using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Notifications.Trakt;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.Trakt.List
{
    public class TraktListImport : TraktImportBase<TraktListSettings>
    {
        public TraktListImport(IImportListRepository importListRepository,
                               ITraktProxy traktProxy,
                               IHttpClient httpClient,
                               IImportListStatusService importListStatusService,
                               IConfigService configService,
                               IParsingService parsingService,
                               Logger logger)
        : base(importListRepository, traktProxy, httpClient, importListStatusService, configService, parsingService, logger)
        {
        }

        public override string Name => "Trakt List";
        public override bool Enabled => true;
        public override bool EnableAuto => false;

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new TraktListRequestGenerator(_traktProxy)
            {
                Settings = Settings
            };
        }
    }
}
