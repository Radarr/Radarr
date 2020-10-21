using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Notifications.Simkl;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.Simkl.User
{
    public class SimklUserImport : SimklImportBase<SimklUserSettings>
    {
        public SimklUserImport(IImportListRepository importListRepository,
                               ISimklProxy simklProxy,
                               IHttpClient httpClient,
                               IImportListStatusService importListStatusService,
                               IConfigService configService,
                               IParsingService parsingService,
                               Logger logger)
        : base(importListRepository, simklProxy, httpClient, importListStatusService, configService, parsingService, logger)
        {
        }

        public override string Name => "Simkl User";
        public override bool Enabled => true;
        public override bool EnableAuto => false;

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new SimklUserRequestGenerator(_SimklProxy)
            {
                Settings = Settings
            };
        }
    }
}
