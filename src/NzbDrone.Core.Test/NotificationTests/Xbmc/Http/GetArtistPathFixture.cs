using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Notifications.Xbmc;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Test.NotificationTests.Xbmc.Http
{
    [TestFixture]
    public class GetArtistPathFixture : CoreTest<HttpApiProvider>
    {
        private XbmcSettings _settings;
        private Artist _artist;

        [SetUp]
        public void Setup()
        {
            _settings = new XbmcSettings
            {
                Host = "localhost",
                Port = 8080,
                Username = "xbmc",
                Password = "xbmc",
                AlwaysUpdate = false,
                CleanLibrary = false,
                UpdateLibrary = true
            };

            _artist = new Artist
            {
                ForeignArtistId = "9f4e41c3-2648-428e-b8c7-dc10465b49ac",
                Name = "Shawn Desman"
            };

            const string setResponseUrl = "http://localhost:8080/xbmcCmds/xbmcHttp?command=SetResponseFormat(webheader;false;webfooter;false;header;<xml>;footer;</xml>;opentag;<tag>;closetag;</tag>;closefinaltag;false)";
            const string resetResponseUrl = "http://localhost:8080/xbmcCmds/xbmcHttp?command=SetResponseFormat()";

            Mocker.GetMock<IHttpProvider>()
                  .Setup(s => s.DownloadString(setResponseUrl, _settings.Username, _settings.Password))
                  .Returns("<xml><tag>OK</xml>");

            Mocker.GetMock<IHttpProvider>()
                  .Setup(s => s.DownloadString(resetResponseUrl, _settings.Username, _settings.Password))
                  .Returns(@"<html>
                             <li>OK
                             </html>");
        }

        [Test]
        public void should_get_artist_path()
        {
            const string queryResult = @"<xml><record><field>smb://xbmc:xbmc@HOMESERVER/Music/Shawn Desman/</field></record></xml>";
            var query = string.Format("http://localhost:8080/xbmcCmds/xbmcHttp?command=QueryMusicDatabase(select path.strPath from path, artist, artistlinkpath where artist.c12 = 9f4e41c3-2648-428e-b8c7-dc10465b49ac and artistlinkpath.idArtist = artist.idArtist and artistlinkpath.idPath = path.idPath)");

            Mocker.GetMock<IHttpProvider>()
                  .Setup(s => s.DownloadString(query, _settings.Username, _settings.Password))
                  .Returns(queryResult);

            Subject.GetArtistPath(_settings, _artist)
                   .Should().Be("smb://xbmc:xbmc@HOMESERVER/Music/Shawn Desman/");
        }

        [Test]
        public void should_get_null_for_artist_path()
        {
            const string queryResult = @"<xml></xml>";
            var query = string.Format("http://localhost:8080/xbmcCmds/xbmcHttp?command=QueryMusicDatabase(select path.strPath from path, artist, artistlinkpath where artist.c12 = 9f4e41c3-2648-428e-b8c7-dc10465b49ac and artistlinkpath.idArtist = artist.idArtist and artistlinkpath.idPath = path.idPath)");

            Mocker.GetMock<IHttpProvider>()
                  .Setup(s => s.DownloadString(query, _settings.Username, _settings.Password))
                  .Returns(queryResult);


            Subject.GetArtistPath(_settings, _artist)
                   .Should().BeNull();
        }

        [Test]
        public void should_get_artist_path_with_special_characters_in_it()
        {
            const string queryResult = @"<xml><record><field>smb://xbmc:xbmc@HOMESERVER/Music/-wumpscut-/</field></record></xml>";
            var query = string.Format("http://localhost:8080/xbmcCmds/xbmcHttp?command=QueryMusicDatabase(select path.strPath from path, artist, artistlinkpath where artist.c12 = 9f4e41c3-2648-428e-b8c7-dc10465b49ac and artistlinkpath.idArtist = artist.idArtist and artistlinkpath.idPath = path.idPath)");

            Mocker.GetMock<IHttpProvider>()
                  .Setup(s => s.DownloadString(query, _settings.Username, _settings.Password))
                  .Returns(queryResult);


            Subject.GetArtistPath(_settings, _artist)
                   .Should().Be("smb://xbmc:xbmc@HOMESERVER/Music/-wumpscut-/");
        }
    }
}
