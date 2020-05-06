using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.LazyLibrarianImport
{
    public class LazyLibrarianImport : HttpImportListBase<LazyLibrarianImportSettings>
    {
        public override string Name => "LazyLibrarian";

        public override ImportListType ListType => ImportListType.Other;

        public override int PageSize => 1000;

        public LazyLibrarianImport(IHttpClient httpClient, IImportListStatusService importListStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, importListStatusService, configService, parsingService, logger)
        {
        }

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new LazyLibrarianImportRequestGenerator { Settings = Settings };
        }

        public override IParseImportListResponse GetParser()
        {
            return new LazyLibrarianImportParser();
        }
    }
}
