using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.HeadphonesImport
{
    public class HeadphonesImport : HttpImportListBase<HeadphonesImportSettings>
    {
        public override string Name => "Headphones";

        public override ImportListType ListType => ImportListType.Other;

        public override int PageSize => 1000;

        public HeadphonesImport(IHttpClient httpClient, IImportListStatusService importListStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, importListStatusService, configService, parsingService, logger)
        {
        }

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new HeadphonesImportRequestGenerator { Settings = Settings};
        }

        public override IParseImportListResponse GetParser()
        {
            return new HeadphonesImportParser();
        }

    }
}
