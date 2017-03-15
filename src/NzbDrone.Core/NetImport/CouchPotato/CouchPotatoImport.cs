using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.NetImport.CouchPotato
{
    public class CouchPotatoImport : HttpNetImportBase<CouchPotatoSettings>
    {
        public override string Name => "CouchPotato";
        public override bool Enabled => true;
        public override bool EnableAuto => false;

        public CouchPotatoImport(IHttpClient httpClient, IConfigService configService, IParsingService parsingService, IProvideMovieIdService movieIdService, Logger logger)
            : base(httpClient, configService, parsingService, movieIdService, logger)
        { }

        public override INetImportRequestGenerator GetRequestGenerator()
        {
            return new CouchPotatoRequestGenerator() { Settings = Settings };
        }

        public override IParseNetImportResponse GetParser()
        {
            return new CouchPotatoParser(Settings);
        }
    }
}
