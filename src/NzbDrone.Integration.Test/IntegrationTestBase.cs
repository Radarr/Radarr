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
using NzbDrone.Core.MediaFiles.BookImport.Manual;
using NzbDrone.Core.Qualities;
using NzbDrone.Integration.Test.Client;
using NzbDrone.SignalR;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.Categories;
using Readarr.Api.V1.Author;
using Readarr.Api.V1.Blacklist;
using Readarr.Api.V1.Books;
using Readarr.Api.V1.Config;
using Readarr.Api.V1.DownloadClient;
using Readarr.Api.V1.History;
using Readarr.Api.V1.Profiles.Quality;
using Readarr.Api.V1.RootFolders;
using Readarr.Api.V1.Tags;
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
        public BookClient Books;
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
        public AuthorClient Author;
        public ClientBase<TagResource> Tags;
        public ClientBase<BookResource> WantedMissing;
        public ClientBase<BookResource> WantedCutoffUnmet;

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

        public abstract string AuthorRootFolder { get; }

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
            Books = new BookClient(RestClient, ApiKey);
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
            Author = new AuthorClient(RestClient, ApiKey);
            Tags = new ClientBase<TagResource>(RestClient, ApiKey);
            WantedMissing = new ClientBase<BookResource>(RestClient, ApiKey, "wanted/missing");
            WantedCutoffUnmet = new ClientBase<BookResource>(RestClient, ApiKey, "wanted/cutoff");
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

        protected async Task ConnectSignalR()
        {
            _signalRReceived = new List<SignalRMessage>();
            _signalrConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:8787/signalr/messages", options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult(ApiKey);
                    })
                .Build();

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
                    await _signalrConnection.StartAsync();
                    connected = true;
                    break;
                }
                catch
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

        public AuthorResource EnsureAuthor(string authorId, string goodreadsBookId, string authorName, bool? monitored = null)
        {
            var result = Author.All().FirstOrDefault(v => v.ForeignAuthorId == authorId);

            if (result == null)
            {
                var lookup = Author.Lookup("readarr:" + goodreadsBookId);
                var author = lookup.First();
                author.QualityProfileId = 1;
                author.MetadataProfileId = 1;
                author.Path = Path.Combine(AuthorRootFolder, author.AuthorName);
                author.Monitored = true;
                author.AddOptions = new Core.Books.AddAuthorOptions();
                Directory.CreateDirectory(author.Path);

                result = Author.Post(author);
                Commands.WaitAll();
                WaitForCompletion(() => Books.GetBooksInAuthor(result.Id).Count > 0);
            }

            var changed = false;

            if (result.RootFolderPath != AuthorRootFolder)
            {
                changed = true;
                result.RootFolderPath = AuthorRootFolder;
                result.Path = Path.Combine(AuthorRootFolder, result.AuthorName);
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
                Author.Put(result);
            }

            return result;
        }

        public void EnsureNoArtist(string readarrId, string artistTitle)
        {
            var result = Author.All().FirstOrDefault(v => v.ForeignAuthorId == readarrId);

            if (result != null)
            {
                Author.Delete(result.Id);
            }
        }

        public void EnsureBookFile(AuthorResource artist, int bookId, int editionId, Quality quality)
        {
            var result = Books.GetBooksInAuthor(artist.Id).Single(v => v.Id == editionId);

            // if (result.BookFile == null)
            if (true)
            {
                var path = Path.Combine(AuthorRootFolder, artist.AuthorName, "Track.mp3");

                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, "Fake Track");

                Commands.PostAndWait(new ManualImportCommand
                {
                    Files = new List<ManualImportFile>
                    {
                            new ManualImportFile
                            {
                                Path = path,
                                AuthorId = artist.Id,
                                BookId = bookId,
                                EditionId = editionId,
                                Quality = new QualityModel(quality)
                            }
                    }
                });
                Commands.WaitAll();

                var track = Books.GetBooksInAuthor(artist.Id).Single(x => x.Id == editionId);

                // track.BookFileId.Should().NotBe(0);
            }
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
