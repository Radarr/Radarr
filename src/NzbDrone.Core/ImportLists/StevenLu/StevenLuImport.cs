using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.StevenLu
{
    public class StevenLuImport : HttpImportListBase<StevenLuSettings>
    {
        public override string Name => "StevenLu Custom";

        public override ImportListSource ListType => ImportListSource.Advanced;
        public override bool Enabled => true;
        public override ImportListType EnableAuto => ImportListType.Manual;

        public StevenLuImport(IHttpClient httpClient,
                              IImportListStatusService importListStatusService,
                              IConfigService configService,
                              IParsingService parsingService,
                              Logger logger)
            : base(httpClient, importListStatusService, configService, parsingService, logger)
        {
        }

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new StevenLuRequestGenerator() { Settings = Settings };
        }

        public override IParseImportListResponse GetParser()
        {
            return new StevenLuParser();
        }
    }
}
