using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests.NewznabTests
{
    [TestFixture]
    public class NewznabFixture : CoreTest<Newznab>
    {
        private NewznabCapabilities _caps;

        [SetUp]
        public void Setup()
        {
            Subject.Definition = new IndexerDefinition()
            {
                Id = 5,
                Name = "Newznab",
                Settings = new NewznabSettings()
                {
                    BaseUrl = "http://indexer.local/",
                    Categories = new int[] { 1 }
                }
            };

            _caps = new NewznabCapabilities();
            Mocker.GetMock<INewznabCapabilitiesProvider>()
                .Setup(v => v.GetCapabilities(It.IsAny<NewznabSettings>()))
                .Returns(_caps);
        }

        [Test]
        public async Task should_parse_recent_feed_from_newznab_nzb_su()
        {
            var recentFeed = ReadAllText(@"Files/Indexers/Newznab/newznab_nzb_su.xml");

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.ExecuteAsync(It.Is<HttpRequest>(v => v.Method == HttpMethod.Get)))
                .Returns<HttpRequest>(r => Task.FromResult(new HttpResponse(r, new HttpHeader(), recentFeed)));

            var releases = await Subject.FetchRecent();

            releases.Should().HaveCount(100);

            var releaseInfo = releases.First();

            releaseInfo.Title.Should().Be("White.Collar.S03E05.720p.HDTV.X264-DIMENSION");
            releaseInfo.DownloadProtocol.Should().Be(DownloadProtocol.Usenet);
            releaseInfo.DownloadUrl.Should().Be("http://nzb.su/getnzb/24967ef4c2e26296c65d3bbfa97aa8fe.nzb&i=37292&r=xxx");
            releaseInfo.InfoUrl.Should().Be("http://nzb.su/details/24967ef4c2e26296c65d3bbfa97aa8fe");
            releaseInfo.CommentUrl.Should().Be("http://nzb.su/details/24967ef4c2e26296c65d3bbfa97aa8fe#comments");
            releaseInfo.IndexerId.Should().Be(Subject.Definition.Id);
            releaseInfo.Indexer.Should().Be(Subject.Definition.Name);
            releaseInfo.PublishDate.Should().Be(DateTime.Parse("2012/02/27 16:09:39"));
            releaseInfo.Size.Should().Be(1183105773);
        }

        public void should_use_best_pagesize_reported_by_caps()
        {
            _caps.MaxPageSize = 30;
            _caps.DefaultPageSize = 25;

            Subject.PageSize.Should().Be(30);
        }

        [Test]
        public void should_not_use_pagesize_over_100_even_if_reported_in_caps()
        {
            _caps.MaxPageSize = 250;
            _caps.DefaultPageSize = 25;

            Subject.PageSize.Should().Be(100);
        }

        [Test]
        public async Task should_parse_languages()
        {
            var recentFeed = ReadAllText(@"Files/Indexers/Newznab/newznab_language.xml");

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.ExecuteAsync(It.Is<HttpRequest>(v => v.Method == HttpMethod.Get)))
                .Returns<HttpRequest>(r => Task.FromResult(new HttpResponse(r, new HttpHeader(), recentFeed)));

            var releases = await Subject.FetchRecent();

            releases.Should().HaveCount(100);

            releases[0].Languages.Should().BeEquivalentTo(new[] { Language.English, Language.Japanese });
            releases[1].Languages.Should().BeEquivalentTo(new[] { Language.English, Language.Spanish });
            releases[2].Languages.Should().BeEquivalentTo(new[] { Language.French });
        }

        [TestCase("no custom attributes")]
        [TestCase("prematch=1 attribute", IndexerFlags.G_Scene)]
        [TestCase("haspretime=1 attribute", IndexerFlags.G_Scene)]
        [TestCase("prematch=0 attribute")]
        [TestCase("haspretime=0 attribute")]
        [TestCase("nuked=1 attribute", IndexerFlags.Nuked)]
        [TestCase("nuked=0 attribute")]
        [TestCase("prematch=1 and nuked=1 attributes", IndexerFlags.G_Scene, IndexerFlags.Nuked)]
        [TestCase("haspretime=0 and nuked=0 attributes")]
        public async Task should_parse_indexer_flags(string releaseGuid, params IndexerFlags[] indexerFlags)
        {
            var feed = ReadAllText(@"Files/Indexers/Newznab/newznab_indexerflags.xml");

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.ExecuteAsync(It.Is<HttpRequest>(v => v.Method == HttpMethod.Get)))
                .Returns<HttpRequest>(r => Task.FromResult(new HttpResponse(r, new HttpHeader(), feed)));

            var releases = await Subject.FetchRecent();

            var release = releases.Should().ContainSingle(r => r.Guid == releaseGuid).Subject;

            indexerFlags.ToList().ForEach(f => release.IndexerFlags.Should().HaveFlag(f));
        }
    }
}
