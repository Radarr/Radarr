using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Transports;
using NLog;
using NLog.Config;
using NLog.Targets;
using NUnit.Framework;
using Radarr.Api.V2.Blacklist;
using Radarr.Api.V2.Commands;
using Radarr.Api.V2.Config;
using Radarr.Api.V2.DownloadClient;
using Radarr.Api.V2.MovieFiles;
using Radarr.Api.V2.History;
using Radarr.Api.V2.Profiles.Quality;
using Radarr.Api.V2.RootFolders;
using Radarr.Api.V2.Movies;
using Radarr.Api.V2.Tags;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Movies.Commands;
using NzbDrone.Integration.Test.Client;
using NzbDrone.SignalR;
using NzbDrone.Test.Common.Categories;
using RestSharp;

namespace NzbDrone.Integration.Test
{
    [IntegrationTest]
    public abstract class IntegrationTestBase
    {
        protected RestClient RestClient { get; private set; }

        public ClientBase<BlacklistResource> Blacklist;
        public CommandClient Commands;
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
        private Connection _signalrConnection;

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
            RestClient = new RestClient(RootUrl + "api/v2/");
            RestClient.AddDefaultHeader("Authentication", ApiKey);
            RestClient.AddDefaultHeader("X-Api-Key", ApiKey);

            Blacklist = new ClientBase<BlacklistResource>(RestClient, ApiKey);
            Commands = new CommandClient(RestClient, ApiKey);
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
            TempDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "_test_" + Process.GetCurrentProcess().Id + "_" + DateTime.UtcNow.Ticks);

            // Wait for things to get quiet, otherwise the previous test might influence the current one.
            Commands.WaitAll();
        }

        [TearDown]
        public void IntegrationTearDown()
        {
            if (_signalrConnection != null)
            {
                switch (_signalrConnection.State)
                {
                    case ConnectionState.Connected:
                    case ConnectionState.Connecting:
                        {
                            _signalrConnection.Stop();
                            break;
                        }
                }

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

        protected void ConnectSignalR()
        {
            _signalRReceived = new List<SignalRMessage>();
            _signalrConnection = new Connection("http://localhost:7878/signalr");
            _signalrConnection.Start(new LongPollingTransport()).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Assert.Fail("SignalrConnection failed. {0}", task.Exception.GetBaseException());
                }
            });

            var retryCount = 0;

            while (_signalrConnection.State != ConnectionState.Connected)
            {
                if (retryCount > 25)
                {
                    Assert.Fail("Couldn't establish signalr connection. State: {0}", _signalrConnection.State);
                }

                retryCount++;
                Console.WriteLine("Connecting to signalR" + _signalrConnection.State);
                Thread.Sleep(200);
            }

            _signalrConnection.Received += json => _signalRReceived.Add(Json.Deserialize<SignalRMessage>(json)); ;
        }

        public static void WaitForCompletion(Func<bool> predicate, int timeout = 10000, int interval = 500)
        {
            var count = timeout / interval;
            for (var i = 0; i < count; i++)
            {
                if (predicate())
                    return;

                Thread.Sleep(interval);
            }

            if (predicate())
                return;

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

                Commands.PostAndWait(new RefreshMovieCommand(movie.Id));
                Commands.WaitAll();

                result = Movies.Get(movie.Id);

                result.MovieFile.Should().NotBeNull();
            }

            return result.MovieFile;
        }

        public QualityProfileResource EnsureProfileCutoff(int profileId, Quality cutoff)
        {
            var profile = Profiles.Get(profileId);

            if (profile.Cutoff != cutoff.Id)
            {
                profile.Cutoff = cutoff.Id;
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
