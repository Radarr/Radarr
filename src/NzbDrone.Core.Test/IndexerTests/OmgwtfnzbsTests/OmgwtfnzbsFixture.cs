using System;
using System.Linq;
using System.Net.Http;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Omgwtfnzbs;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests.OmgwtfnzbsTests
{
    [TestFixture]
    public class OmgwtfnzbsFixture : CoreTest<Omgwtfnzbs>
    {
        [SetUp]
        public void Setup()
        {
            Subject.Definition = new IndexerDefinition()
            {
                Name = "Omgwtfnzbs",
                Settings = new OmgwtfnzbsSettings()
                {
                    ApiKey = "xxx",
                    Username = "me@my.domain"
                }
            };
        }

        [Test]
        public void should_parse_recent_feed_from_omgwtfnzbs()
        {
            var recentFeed = ReadAllText(@"Files/Indexers/Omgwtfnzbs/Omgwtfnzbs.xml");

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.Get)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed));

            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(100);

            var releaseInfo = releases.First();

            releaseInfo.Title.Should().Be("Un.Petit.Boulot.2016.FRENCH.720p.BluRay.DTS.x264-LOST");
            releaseInfo.DownloadProtocol.Should().Be(DownloadProtocol.Usenet);
            releaseInfo.DownloadUrl.Should().Be("https://api.omgwtfnzbs.me/nzb/?id=8a2Bw&user=nzbdrone&api=nzbdrone");
            releaseInfo.InfoUrl.Should().Be("https://omgwtfnzbs.me/details.php?id=8a2Bw");
            releaseInfo.CommentUrl.Should().BeNullOrEmpty();
            releaseInfo.Indexer.Should().Be(Subject.Definition.Name);
            releaseInfo.PublishDate.Should().Be(DateTime.Parse("2017/01/09 00:16:54"));
            releaseInfo.Size.Should().Be(5354909355);
        }
    }
}
