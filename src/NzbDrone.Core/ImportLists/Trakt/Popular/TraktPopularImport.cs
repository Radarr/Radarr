using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Notifications.Trakt;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.Trakt.Popular
{
    public class TraktPopularImport : TraktImportBase<TraktPopularSettings>
    {
        public TraktPopularImport(IImportListRepository importListRepository,
                                  ITraktProxy traktProxy,
                                  IHttpClient httpClient,
                                  IImportListStatusService importListStatusService,
                                  IConfigService configService,
                                  IParsingService parsingService,
                                  Logger logger)
        : base(importListRepository, traktProxy, httpClient, importListStatusService, configService, parsingService, logger)
        {
        }

        public override string Name => "Trakt Popular List";
        public override bool Enabled => true;
        public override bool EnableAuto => false;

        public override IParseImportListResponse GetParser()
        {
            return new TraktPopularParser(Settings);
        }

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new TraktPopularRequestGenerator(_traktProxy)
            {
                Settings = Settings
            };
        }
    }
}
