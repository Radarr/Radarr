using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Notifications.Simkl;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.Simkl.Popular
{
    public class SimklPopularImport : SimklImportBase<SimklPopularSettings>
    {
        public SimklPopularImport(IImportListRepository importListRepository,
                                  ISimklProxy simklProxy,
                                  IHttpClient httpClient,
                                  IImportListStatusService importListStatusService,
                                  IConfigService configService,
                                  IParsingService parsingService,
                                  Logger logger)
        : base(importListRepository, simklProxy, httpClient, importListStatusService, configService, parsingService, logger)
        {
        }

        public override string Name => "Simkl Popular List";
        public override bool Enabled => true;
        public override bool EnableAuto => false;

        public override IParseImportListResponse GetParser()
        {
            return new SimklPopularParser(Settings);
        }

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new SimklPopularRequestGenerator(_SimklProxy)
            {
                Settings = Settings
            };
        }
    }
}
