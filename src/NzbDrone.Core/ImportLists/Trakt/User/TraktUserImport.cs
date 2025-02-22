using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Notifications.Trakt;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.Trakt.User
{
    public class TraktUserImport : TraktImportBase<TraktUserSettings>
    {
        public TraktUserImport(IImportListRepository importListRepository,
                               ITraktProxy traktProxy,
                               IHttpClient httpClient,
                               IImportListStatusService importListStatusService,
                               IConfigService configService,
                               IParsingService parsingService,
                               Logger logger)
        : base(importListRepository, traktProxy, httpClient, importListStatusService, configService, parsingService, logger)
        {
        }

        public override string Name => "Trakt User";
        public override bool Enabled => true;
        public override bool EnableAuto => false;

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new TraktUserRequestGenerator(_traktProxy, Settings);
        }
    }
}
