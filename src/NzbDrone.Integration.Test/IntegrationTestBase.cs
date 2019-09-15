using System;
using System.Collections.Generic;
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
using Lidarr.Api.V1.Blacklist;
using Lidarr.Api.V1.Config;
using Lidarr.Api.V1.DownloadClient;
using Lidarr.Api.V1.History;
using Lidarr.Api.V1.Profiles.Quality;
using Lidarr.Api.V1.RootFolders;
using Lidarr.Api.V1.Artist;
using Lidarr.Api.V1.Albums;
using Lidarr.Api.V1.Tags;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Qualities;
using NzbDrone.Integration.Test.Client;
using NzbDrone.SignalR;
using NzbDrone.Test.Common.Categories;
using RestSharp;
using NzbDrone.Core.MediaFiles.TrackImport.Manual;
using NzbDrone.Test.Common;

namespace NzbDrone.Integration.Test
{
    [IntegrationTest]
    public abstract class IntegrationTestBase
    {
        protected RestClient RestClient { get; private set; }

        public ClientBase<BlacklistResource> Blacklist;
        public CommandClient Commands;
        public DownloadClientClient DownloadClients;
        public AlbumClient Albums;
        public TrackClient Tracks;
        public ClientBase<HistoryResource> History;
        public ClientBase<HostConfigResource> HostConfig;
        public IndexerClient Indexers;
        public LogsClient Logs;
        public ClientBase<NamingConfigResource> NamingConfig;
        public NotificationClient Notifications;
        public ClientBase<QualityProfileResource> Profiles;
        public ReleaseClient Releases;
        public ReleasePushClient ReleasePush;
        public ClientBase<RootFolderResource> RootFolders;
        public ArtistClient Artist;
        public ClientBase<TagResource> Tags;
        public ClientBase<AlbumResource> WantedMissing;
        public ClientBase<AlbumResource> WantedCutoffUnmet;

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

        public abstract string ArtistRootFolder { get; }

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
            RestClient = new RestClient(RootUrl + "api/v1/");
            RestClient.AddDefaultHeader("Authentication", ApiKey);
            RestClient.AddDefaultHeader("X-Api-Key", ApiKey);

            Blacklist = new ClientBase<BlacklistResource>(RestClient, ApiKey);
            Commands = new CommandClient(RestClient, ApiKey);
            DownloadClients = new DownloadClientClient(RestClient, ApiKey);
            Albums = new AlbumClient(RestClient, ApiKey);
            Tracks = new TrackClient(RestClient, ApiKey);
            History = new ClientBase<HistoryResource>(RestClient, ApiKey);
            HostConfig = new ClientBase<HostConfigResource>(RestClient, ApiKey, "config/host");
            Indexers = new IndexerClient(RestClient, ApiKey);
            Logs = new LogsClient(RestClient, ApiKey);
            NamingConfig = new ClientBase<NamingConfigResource>(RestClient, ApiKey, "config/naming");
            Notifications = new NotificationClient(RestClient, ApiKey);
            Profiles = new ClientBase<QualityProfileResource>(RestClient, ApiKey);
            Releases = new ReleaseClient(RestClient, ApiKey);
            ReleasePush = new ReleasePushClient(RestClient, ApiKey);
            RootFolders = new ClientBase<RootFolderResource>(RestClient, ApiKey);
            Artist = new ArtistClient(RestClient, ApiKey);
            Tags = new ClientBase<TagResource>(RestClient, ApiKey);
            WantedMissing = new ClientBase<AlbumResource>(RestClient, ApiKey, "wanted/missing");
            WantedCutoffUnmet = new ClientBase<AlbumResource>(RestClient, ApiKey, "wanted/cutoff");
        }

        [OneTimeTearDown]
        public void SmokeTestTearDown()
        {
            StopTestTarget();
        }

        [SetUp]
        public void IntegrationSetUp()
        {
            TempDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "_test_" + TestBase.GetUID());

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

        protected void IgnoreOnMonoVersions(params string[] version_strings)
        {
            if (!PlatformInfo.IsMono)
            {
                return;
            }

            var current = PlatformInfo.GetVersion();
            var versions = version_strings.Select(x => new Version(x)).ToList();

            if (versions.Any(x => x.Major == current.Major && x.Minor == current.Minor))
            {
                throw new IgnoreException($"Ignored on mono {PlatformInfo.GetVersion()}");
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
            _signalrConnection = new Connection("http://localhost:8686/signalr");
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

        public ArtistResource EnsureArtist(string lidarrId, string artistName, bool? monitored = null)
        {
            var result = Artist.All().FirstOrDefault(v => v.ForeignArtistId == lidarrId);

            if (result == null)
            {
                var lookup = Artist.Lookup("lidarr:" + lidarrId);
                var artist = lookup.First();
                artist.QualityProfileId = 1;
                artist.MetadataProfileId = 1;
                artist.Path = Path.Combine(ArtistRootFolder, artist.ArtistName);
                artist.Monitored = true;
                artist.AddOptions = new Core.Music.AddArtistOptions();
                Directory.CreateDirectory(artist.Path);

                result = Artist.Post(artist);
                Commands.WaitAll();
                WaitForCompletion(() => Tracks.GetTracksInArtist(result.Id).Count > 0);
            }

            var changed = false;

            if (result.RootFolderPath != ArtistRootFolder)
            {
                changed = true;
                result.RootFolderPath = ArtistRootFolder;
                result.Path = Path.Combine(ArtistRootFolder, result.ArtistName);
            }

            if (monitored.HasValue)
            {
                if (result.Monitored != monitored.Value)
                {
                    result.Monitored = monitored.Value;
                    changed = true;
                }
            }

            if (changed)
            {
                Artist.Put(result);
            }

            return result;
        }


        public void EnsureNoArtist(string lidarrId, string artistTitle)
        {
            var result = Artist.All().FirstOrDefault(v => v.ForeignArtistId == lidarrId);

            if (result != null)
            {
                Artist.Delete(result.Id);
            }
        }

        public void EnsureTrackFile(ArtistResource artist, int albumId, int albumReleaseId, int trackId, Quality quality)
        {
            var result = Tracks.GetTracksInArtist(artist.Id).Single(v => v.Id == trackId);

            if (result.TrackFile == null)
            {
                var path = Path.Combine(ArtistRootFolder, artist.ArtistName, "Track.mp3");

                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, "Fake Track");

                Commands.PostAndWait(new ManualImportCommand {
                        Files = new List<ManualImportFile> {
                            new ManualImportFile {
                                Path = path,
                                ArtistId = artist.Id,
                                AlbumId = albumId,
                                AlbumReleaseId = albumReleaseId,
                                TrackIds = new List<int> { trackId },
                                Quality = new QualityModel(quality)
                            }
                        }
                    });
                Commands.WaitAll();
                
                var track = Tracks.GetTracksInArtist(artist.Id).Single(x => x.Id == trackId);

                track.TrackFileId.Should().NotBe(0);
            }
        }

        public QualityProfileResource EnsureProfileCutoff(int profileId, string cutoff)
        {
            var profile = Profiles.Get(profileId);
            var cutoffItem = profile.Items.First(x => x.Name == cutoff);

            if (profile.Cutoff != cutoffItem.Id)
            {
                profile.Cutoff = cutoffItem.Id;
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
