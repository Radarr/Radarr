using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.NetImport.CouchPotato
{
    public class CouchPotatoImport : HttpNetImportBase<CouchPotatoSettings>
    {
        public override string Name => "CouchPotato";
        public override bool Enabled => true;
        public override bool EnableAuto => false;

        public CouchPotatoImport(IHttpClient httpClient, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, configService, parsingService, logger)
        { }

        /*public new virtual IEnumerable<ProviderDefinition> DefaultDefinitions
        {
            get
            {
                var config = (CouchPotatoSettings)new CouchPotatoSettings();
                config.Link = "http://localhost";
                config.Port = "5050";

                yield return new NetImportDefinition
                {
                    Name = "Localhost",
                    Enabled = config.Validate().IsValid && Enabled,
                    Implementation = GetType().Name,
                    Settings = config
                };
            }
        }*/

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
