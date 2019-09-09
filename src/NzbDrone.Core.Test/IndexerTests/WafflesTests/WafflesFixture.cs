using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Waffles;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests.WafflesTests
{
    [TestFixture]
    public class WafflesFixture : CoreTest<Waffles>
    {
        [SetUp]
        public void Setup()
        {
            Subject.Definition = new IndexerDefinition()
                {
                    Name = "Waffles",
                    Settings = new WafflesSettings()
                        {
                            UserId = "xxx",
                            RssPasskey = "123456789"
                        }
                };
        }

        [Test]
        public void should_parse_recent_feed_from_waffles()
        {
            var recentFeed = ReadAllText(@"Files/Indexers/Waffles/waffles.xml");

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.GET)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed));
            
            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(15);

            var releaseInfo = releases.First();

            releaseInfo.Title.Should().Be("Coldplay - Kaleidoscope EP (FLAC HD) [2017-Web-FLAC-Lossless]");
            releaseInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            releaseInfo.DownloadUrl.Should().Be("https://waffles.ch/download.php/xxx/1166992/" +
                "Coldplay%20-%20Kaleidoscope%20EP%20%28FLAC%20HD%29%20%5B2017-Web-FLAC-Lossless%5D.torrent?passkey=123456789&uid=xxx&rss=1");
            releaseInfo.InfoUrl.Should().Be("https://waffles.ch/details.php?id=1166992&hit=1");
            releaseInfo.CommentUrl.Should().Be("https://waffles.ch/details.php?id=1166992&hit=1");
            releaseInfo.Indexer.Should().Be(Subject.Definition.Name);
            releaseInfo.PublishDate.Should().Be(DateTime.Parse("2017-07-16 09:51:54"));
            releaseInfo.Size.Should().Be(552668227);
        }
    }
}
