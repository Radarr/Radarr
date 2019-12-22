using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.NetImport;
using NzbDrone.Core.NetImport.CouchPotato;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.NetImport.CouchPotato
{
    public class CouchPotatoTest : CoreTest<CouchPotatoParser>
    {
        private NetImportResponse CreateResponse(string url, string content)
        {
            var httpRequest = new HttpRequest(url);
            var httpResponse = new HttpResponse(httpRequest, new HttpHeader(), Encoding.UTF8.GetBytes(content));

            return new NetImportResponse(new NetImportRequest(httpRequest), httpResponse);
        }


        [Test]
        public void should_parse_json_of_couchpotato()
        {
            var json = ReadAllText("Files/couchpotato_movie_list.json");

            var result = Subject.ParseResponse(CreateResponse("http://my.indexer.com/api?q=My+Favourite+Show", json));

            result.First().Title.Should().Be("Rogue One: A Star Wars Story");
            result.First().ImdbId.Should().Be("tt3748528");
        }
    }
}
