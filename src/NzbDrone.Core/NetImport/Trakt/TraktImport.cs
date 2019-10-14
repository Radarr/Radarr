using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.NetImport.Trakt
{
    public class TraktImport : HttpNetImportBase<TraktSettings>
    {
        public override string Name => "Trakt List";
        public override bool Enabled => true;
        public override bool EnableAuto => false;

        public TraktImport(IHttpClient httpClient, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, configService, parsingService, logger)
        {
        }

        public override INetImportRequestGenerator GetRequestGenerator()
        {
            return new TraktRequestGenerator() { Settings = Settings, _configService=_configService, HttpClient = _httpClient, };
        }

        public override IParseNetImportResponse GetParser()
        {
            return new TraktParser(Settings);
        }
    }
}
