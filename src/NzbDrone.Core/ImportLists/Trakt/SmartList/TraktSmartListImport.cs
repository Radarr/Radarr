using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Notifications.Trakt;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.Trakt.SmartList
{
    public class TraktSmartListImport : TraktImportBase<TraktSmartListSettings>
    {
        public TraktSmartListImport(IImportListRepository importListRepository,
                                    ITraktProxy traktProxy,
                                    IHttpClient httpClient,
                                    IImportListStatusService importListStatusService,
                                    IConfigService configService,
                                    IParsingService parsingService,
                                    Logger logger)
        : base(importListRepository, traktProxy, httpClient, importListStatusService, configService, parsingService, logger)
        {
        }

        public override string Name => "Trakt Smart List";
        public override bool Enabled => true;
        public override bool EnableAuto => false;

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new TraktSmartListRequestGenerator(_traktProxy)
            {
                Settings = Settings
            };
        }
    }
}
