using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.CouchPotato
{
    public class CouchPotatoImport : HttpImportListBase<CouchPotatoSettings>
    {
        public override string Name => "CouchPotato";

        public override ImportListType ListType => ImportListType.Program;
        public override bool Enabled => true;
        public override bool EnableAuto => false;

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
