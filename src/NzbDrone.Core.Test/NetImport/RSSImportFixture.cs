using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.NetImport;
using NzbDrone.Core.NetImport.RSSImport;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.NetImport
{
    [TestFixture]
    public class RSSImportFixture : CoreTest<RSSImport>
    {

        [SetUp]
        public void Setup()
        {
            Subject.Definition = Subject.GetDefaultDefinitions().First();
        }
        private void GivenRecentFeedResponse(string rssXmlFile)
        {
            var recentFeed = ReadAllText(@"Files/" + rssXmlFile);

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.IsAny<HttpRequest>()))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed));
        }

        [Test]
        public void should_fetch_imdb_list()
        {
            GivenRecentFeedResponse("imdb_watchlist.xml");

            var result = Subject.Fetch();

            result.Movies.First().Title.Should().Be("Think Like a Man Too");
            result.Movies.First().ImdbId.Should().Be("tt2239832");
        }
    }
}