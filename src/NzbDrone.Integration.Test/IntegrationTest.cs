using System.Collections.Generic;
using NLog;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Test.Common;
using Radarr.Http.ClientSchema;

namespace NzbDrone.Integration.Test
{
    public abstract class IntegrationTest : IntegrationTestBase
    {
        protected NzbDroneRunner _runner;

        public override string MovieRootFolder => GetTempDirectory("MovieRootFolder");

        protected override string RootUrl => "http://localhost:7878/";

        protected override string ApiKey => _runner.ApiKey;

        protected override void StartTestTarget()
        {
            _runner = new NzbDroneRunner(LogManager.GetCurrentClassLogger());
            _runner.KillAll();

            _runner.Start();
        }

        protected override void InitializeTestTarget()
        {
            Indexers.Post(new Api.Indexers.IndexerResource
            {
                EnableRss = false,
                EnableSearch = false,
                ConfigContract = nameof(NewznabSettings),
                Implementation = nameof(Newznab),
                Name = "NewznabTest",
                Protocol = Core.Indexers.DownloadProtocol.Usenet,
                Fields = SchemaBuilder.ToSchema(new NewznabSettings())
            });
        }

        protected override void StopTestTarget()
        {
            _runner.KillAll();
        }
    }
}
