using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Newznab;
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
                            Url = "http://indexer.local/",
                            Categories = new int[] { 1 }
                        }
                };

            _caps = new NewznabCapabilities();
            Mocker.GetMock<INewznabCapabilitiesProvider>()
                .Setup(v => v.GetCapabilities(It.IsAny<NewznabSettings>()))
                .Returns(_caps);
        }

        [Test]
        public void should_parse_recent_feed_from_newznab_nzb_su()
        {
            var recentFeed = ReadAllText(@"Files/Indexers/Newznab/newznab_nzb_su.xml");

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.GET)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed));
            
            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(100);

            var releaseInfo = releases.First();

            releaseInfo.Title.Should().Be("Brainstorm-Scary Creatures-CD-FLAC-2016-NBFLAC");
            releaseInfo.DownloadProtocol.Should().Be(DownloadProtocol.Usenet);
            releaseInfo.DownloadUrl.Should().Be("https://api.nzbgeek.info/api?t=get&id=38884827e1e56b9336278a449e0a38ec&apikey=xxx");
            releaseInfo.InfoUrl.Should().Be("https://nzbgeek.info/geekseek.php?guid=38884827e1e56b9336278a449e0a38ec");
            releaseInfo.CommentUrl.Should().Be("https://nzbgeek.info/geekseek.php?guid=38884827e1e56b9336278a449e0a38ec");
            releaseInfo.IndexerId.Should().Be(Subject.Definition.Id);
            releaseInfo.Indexer.Should().Be(Subject.Definition.Name);
            releaseInfo.PublishDate.Should().Be(DateTime.Parse("2017/05/26 05:54:31"));
            releaseInfo.Size.Should().Be(492735000);
        }

        [Test]
        public void should_use_pagesize_reported_by_caps()
        {
            _caps.MaxPageSize = 30;
            _caps.DefaultPageSize = 25;

            Subject.PageSize.Should().Be(25);
        }
    }
}
