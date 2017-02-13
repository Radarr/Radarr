using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.PassThePopcorn;
using NzbDrone.Core.Parser;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.NetImport.TMDb
{
    public class TMDbImport : HttpNetImportBase<TMDbSettings>
    {
        public override string Name => "TMDb Lists";
        public override bool Enabled => true;
        public override bool EnableAuto => false;

        public TMDbImport(IHttpClient httpClient, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, configService, parsingService, logger)
        { }

        public override INetImportRequestGenerator GetRequestGenerator()
        {
            return new TMDbRequestGenerator() { Settings = Settings };
        }

        public override IParseNetImportResponse GetParser()
        {
            return new TMDbParser(Settings);
        }
    }
}
