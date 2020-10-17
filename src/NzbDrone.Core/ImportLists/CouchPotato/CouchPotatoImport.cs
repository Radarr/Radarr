using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.CouchPotato
{
    public class CouchPotatoImport : HttpImportListBase<CouchPotatoSettings>
    {
        public override string Name => "CouchPotato";

        public override ImportListSource ListType => ImportListSource.Program;
        public override bool Enabled => true;
        public override ImportListType EnableAuto => ImportListType.Manual;

        public CouchPotatoImport(IHttpClient httpClient, IImportListStatusService importListStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, importListStatusService, configService, parsingService, logger)
        {
        }

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new CouchPotatoRequestGenerator() { Settings = Settings };
        }

        public override IParseImportListResponse GetParser()
        {
            return new CouchPotatoParser();
        }
    }
}
