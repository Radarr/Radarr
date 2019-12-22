using System.Linq;
using System.Text;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.NetImport;
using NzbDrone.Core.NetImport.RSSImport;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.NetImport
{
    public class RSSImportTest : CoreTest<RSSImportParser>
    {
        private NetImportResponse CreateResponse(string url, string content)
        {
            var httpRequest = new HttpRequest(url);
            var httpResponse = new HttpResponse(httpRequest, new HttpHeader(), Encoding.UTF8.GetBytes(content));

            return new NetImportResponse(new NetImportRequest(httpRequest), httpResponse);
        }


        [Test]
        public void should_parse_xml_of_imdb()
        {
            var xml = ReadAllText("Files/imdb_watchlist.xml");

            var result = Subject.ParseResponse(CreateResponse("http://my.indexer.com/api?q=My+Favourite+Show", xml));

            result.First().Title.Should().Be("Think Like a Man Too");
            result.First().ImdbId.Should().Be("tt2239832");
        }
    }
}
