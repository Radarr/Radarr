using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.LastFm
{
    public class LastFmUser : HttpImportListBase<LastFmUserSettings>
    {
        public override string Name => "Last.fm User";

        public override ImportListType ListType => ImportListType.LastFm;

        public override int PageSize => 1000;

        public LastFmUser(IHttpClient httpClient, IImportListStatusService importListStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, importListStatusService, configService, parsingService, logger)
        {
        }

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new LastFmUserRequestGenerator { Settings = Settings};
        }

        public override IParseImportListResponse GetParser()
        {
            return new LastFmParser();
        }

    }
}
