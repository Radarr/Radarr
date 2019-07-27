using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.LastFm
{
    public class LastFmTag : HttpImportListBase<LastFmTagSettings>
    {
        public override string Name => "Last.fm Tag";

        public override ImportListType ListType => ImportListType.LastFm;

        public override int PageSize => 1000;

        public LastFmTag(IHttpClient httpClient, IImportListStatusService importListStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, importListStatusService, configService, parsingService, logger)
        {
        }

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new LastFmTagRequestGenerator { Settings = Settings};
        }

        public override IParseImportListResponse GetParser()
        {
            return new LastFmParser();
        }

    }
}
