using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Http.CloudFlare;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.NetImport
{
    public abstract class HttpNetImportBase<TSettings> : NetImportBase<TSettings>
        where TSettings : IProviderConfig, new()
    {
        protected const int MaxNumResultsPerQuery = 1000;

        protected readonly IHttpClient _httpClient;

        public override bool Enabled => true;

        public virtual TimeSpan RateLimit => TimeSpan.FromSeconds(2);

        public abstract INetImportRequestGenerator GetRequestGenerator();
        public abstract IParseNetImportResponse GetParser();

        public HttpNetImportBase(IHttpClient httpClient, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(configService, parsingService, logger)
        {
            _httpClient = httpClient;
        }

        public override IList<Movie> Fetch()
        {
            return new List<Movie>();
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            throw new NotImplementedException();
        }

        protected virtual ValidationFailure TestConnection()
        {
            throw new NotImplementedException();
        }
    }

}
