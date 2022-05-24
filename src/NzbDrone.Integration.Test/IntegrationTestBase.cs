using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;
using NLog;
using NLog.Config;
using NLog.Targets;
using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Processes;
using NzbDrone.Core.Movies.Commands;
using NzbDrone.Core.Qualities;
using NzbDrone.Integration.Test.Client;
using NzbDrone.SignalR;
using NzbDrone.Test.Common.Categories;
using Radarr.Api.V3.Blocklist;
using Radarr.Api.V3.Config;
using Radarr.Api.V3.DownloadClient;
using Radarr.Api.V3.History;
using Radarr.Api.V3.MovieFiles;
using Radarr.Api.V3.Movies;
using Radarr.Api.V3.Profiles.Quality;
using Radarr.Api.V3.RootFolders;
using Radarr.Api.V3.System.Tasks;
using Radarr.Api.V3.Tags;
using RestSharp;

namespace NzbDrone.Integration.Test
{
    [IntegrationTest]
    public abstract class IntegrationTestBase
    {
        protected RestClient RestClient { get; private set; }

        public ClientBase<BlocklistResource> Blocklist;
        public CommandClient Commands;
        public ClientBase<TaskResource> Tasks;
        public DownloadClientClient DownloadClients;
        public ClientBase<HistoryResource> History;
        public ClientBase<HostConfigResource> HostConfig;
        public IndexerClient Indexers;
        public LogsClient Logs;
        public ClientBase<NamingConfigResource> NamingConfig;
        public NotificationClient Notifications;
        public ClientBase<QualityProfileResource> Profiles;
        public ReleaseClient Releases;
        public ClientBase<RootFolderResource> RootFolders;
        public MovieClient Movies;
        public ClientBase<TagResource> Tags;
        public ClientBase<MovieResource> WantedMissing;
        public ClientBase<MovieResource> WantedCutoffUnmet;

        private List<SignalRMessage> _signalRReceived;

        private HubConnection _signalrConnection;

        protected IEnumerable<SignalRMessage> SignalRMessages => _signalRReceived;

        public IntegrationTestBase()
        {
            new StartupContext();

            LogManager.Configuration = new LoggingConfiguration();
            var consoleTarget = new ConsoleTarget { Layout = "${level}: ${message} ${exception}" };
            LogManager.Configuration.AddTarget(consoleTarget.GetType().Name, consoleTarget);
            LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, consoleTarget));
        }

        public string TempDirectory { get; private set; }

        public abstract string MovieRootFolder { get; }

        protected abstract string RootUrl { get; }

        protected abstract string ApiKey { get; }

        protected abstract void StartTestTarget();

        protected abstract void InitializeTestTarget();

        protected abstract void StopTestTarget();

        [OneTimeSetUp]
        public void SmokeTestSetup()
        {
            StartTestTarget();
            InitRestClients();
            InitializeTestTarget();
        }

        protected virtual void InitRestClients()
        {
            RestClient = new RestClient(RootUrl + "api/v3/");
            RestClient.AddDefaultHeader("Authentication", ApiKey);
            RestClient.AddDefaultHeader("X-Api-Key", ApiKey);

            Blocklist = new ClientBase<BlocklistResource>(RestClient, ApiKey);
            Commands = new CommandClient(RestClient, ApiKey);
            Tasks = new ClientBase<TaskResource>(RestClient, ApiKey, "system/task");
            DownloadClients = new DownloadClientClient(RestClient, ApiKey);
            History = new ClientBase<HistoryResource>(RestClient, ApiKey);
            HostConfig = new ClientBase<HostConfigResource>(RestClient, ApiKey, "config/host");
            Indexers = new IndexerClient(RestClient, ApiKey);
            Logs = new LogsClient(RestClient, ApiKey);
            NamingConfig = new ClientBase<NamingConfigResource>(RestClient, ApiKey, "config/naming");
            Notifications = new NotificationClient(RestClient, ApiKey);
            Profiles = new ClientBase<QualityProfileResource>(RestClient, ApiKey);
            Releases = new ReleaseClient(RestClient, ApiKey);
            RootFolders = new ClientBase<RootFolderResource>(RestClient, ApiKey);
            Movies = new MovieClient(RestClient, ApiKey);
            Tags = new ClientBase<TagResource>(RestClient, ApiKey);
            WantedMissing = new ClientBase<MovieResource>(RestClient, ApiKey, "wanted/missing");
            WantedCutoffUnmet = new ClientBase<MovieResource>(RestClient, ApiKey, "wanted/cutoff");
        }

        [OneTimeTearDown]
        public void SmokeTestTearDown()
        {
            StopTestTarget();
        }

        [SetUp]
        public void IntegrationSetUp()
        {
            TempDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "_test_" + ProcessProvider.GetCurrentProcessId() + "_" + DateTime.UtcNow.Ticks);

            // Wait for things to get quiet, otherwise the previous test might influence the current one.
            Commands.WaitAll();
        }

        [TearDown]
        public async Task IntegrationTearDown()
        {
            if (_signalrConnection != null)
            {
                await _signalrConnection.StopAsync();

                _signalrConnection = null;
                _signalRReceived = new List<SignalRMessage>();
            }

            if (Directory.Exists(TempDirectory))
            {
                try
                {
                    Directory.Delete(TempDirectory, true);
                }
                catch
                {
                }
            }
        }

        public string GetTempDirectory(params string[] args)
        {
            var path = Path.Combine(TempDirectory, Path.Combine(args));

            Directory.CreateDirectory(path);

            return path;
        }

        protected async Task ConnectSignalR()
        {
            _signalRReceived = new List<SignalRMessage>();
            _signalrConnection = new HubConnectionBuilder().WithUrl("http://localhost:7878/signalr/messages").Build();

            var cts = new CancellationTokenSource();

            _signalrConnection.Closed += e =>
            {
                cts.Cancel();
                return Task.CompletedTask;
            };

            _signalrConnection.On<SignalRMessage>("receiveMessage", (message) =>
            {
                _signalRReceived.Add(message);
            });

            var connected = false;
            var retryCount = 0;

            while (!connected)
            {
                try
                {
                    Console.WriteLine("Connecting to signalR");

                    await _signalrConnection.StartAsync();
                    connected = true;
                    break;
                }
                catch (Exception)
                {
                    if (retryCount > 25)
                    {
                        Assert.Fail("Couldn't establish signalR connection");
                    }
                }

                retryCount++;
                Thread.Sleep(200);
            }
        }

        public static void WaitForCompletion(Func<bool> predicate, int timeout = 10000, int interval = 500)
        {
            var count = timeout / interval;
            for (var i = 0; i < count; i++)
            {
                if (predicate())
                {
                    return;
                }

                Thread.Sleep(interval);
            }

            if (predicate())
            {
                return;
            }

            Assert.Fail("Timed on wait");
        }

        public MovieResource EnsureMovie(int tmdbid, string movieTitle, bool? monitored = null)
        {
            var result = Movies.All().FirstOrDefault(v => v.TmdbId == tmdbid);

            if (result == null)
            {
                var lookup = Movies.Lookup("tmdb:" + tmdbid);
                var movie = lookup.First();
                movie.QualityProfileId = 1;
                movie.Path = Path.Combine(MovieRootFolder, movie.Title);
                movie.Monitored = true;
                movie.AddOptions = new Core.Movies.AddMovieOptions();
                Directory.CreateDirectory(movie.Path);

                result = Movies.Post(movie);
                Commands.WaitAll();
            }

            if (monitored.HasValue)
            {
                var changed = false;
                if (result.Monitored != monitored.Value)
                {
                    result.Monitored = monitored.Value;
                    changed = true;
                }

                if (changed)
                {
                    Movies.Put(result);
                }
            }

            Commands.WaitAll();

            return result;
        }

        public void EnsureNoMovie(int tmdbid, string movieTitle)
        {
            var result = Movies.All().FirstOrDefault(v => v.TmdbId == tmdbid);

            if (result != null)
            {
                Movies.Delete(result.Id);
            }
        }

        public MovieFileResource EnsureMovieFile(MovieResource movie, Quality quality)
        {
            var result = Movies.Get(movie.Id);

            if (result.MovieFile == null)
            {
                var path = Path.Combine(MovieRootFolder, movie.Title, string.Format("{0} ({1}) - {2}.strm", movie.Title, movie.Year, quality.Name));

                Directory.CreateDirectory(Path.GetDirectoryName(path));

                var sourcePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "ApiTests", "Files", "H264_sample.mp4");

                //File.Copy(sourcePath, path);
                File.WriteAllText(path, "Fake Movie");

                Commands.PostAndWait(new RefreshMovieCommand(new List<int> { movie.Id }));
                Commands.WaitAll();

                result = Movies.Get(movie.Id);

                result.MovieFile.Should().NotBeNull();
            }

            return result.MovieFile;
        }

        public QualityProfileResource EnsureProfileCutoff(int profileId, Quality cutoff, bool upgradeAllowed)
        {
            var needsUpdate = false;
            var profile = Profiles.Get(profileId);

            if (profile.Cutoff != cutoff.Id)
            {
                profile.Cutoff = cutoff.Id;
                needsUpdate = true;
            }

            if (profile.UpgradeAllowed != upgradeAllowed)
            {
                profile.UpgradeAllowed = upgradeAllowed;
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                profile = Profiles.Put(profile);
            }

            return profile;
        }

        public TagResource EnsureTag(string tagLabel)
        {
            var tag = Tags.All().FirstOrDefault(v => v.Label == tagLabel);

            if (tag == null)
            {
                tag = Tags.Post(new TagResource { Label = tagLabel });
            }

            return tag;
        }

        public void EnsureNoTag(string tagLabel)
        {
            var tag = Tags.All().FirstOrDefault(v => v.Label == tagLabel);

            if (tag != null)
            {
                Tags.Delete(tag.Id);
            }
        }

        public DownloadClientResource EnsureDownloadClient(bool enabled = true)
        {
            var client = DownloadClients.All().FirstOrDefault(v => v.Name == "Test UsenetBlackhole");

            if (client == null)
            {
                var schema = DownloadClients.Schema().First(v => v.Implementation == "UsenetBlackhole");

                schema.Enable = enabled;
                schema.Name = "Test UsenetBlackhole";
                schema.Fields.First(v => v.Name == "watchFolder").Value = GetTempDirectory("Download", "UsenetBlackhole", "Watch");
                schema.Fields.First(v => v.Name == "nzbFolder").Value = GetTempDirectory("Download", "UsenetBlackhole", "Nzb");

                client = DownloadClients.Post(schema);
            }
            else if (client.Enable != enabled)
            {
                client.Enable = enabled;

                client = DownloadClients.Put(client);
            }

            return client;
        }

        public void EnsureNoDownloadClient()
        {
            var clients = DownloadClients.All();

            foreach (var client in clients)
            {
                DownloadClients.Delete(client.Id);
            }
        }
    }
}
