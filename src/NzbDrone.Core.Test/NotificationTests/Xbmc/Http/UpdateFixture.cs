using FizzWare.NBuilder;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Notifications.Xbmc;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Test.NotificationTests.Xbmc.Http
{
    [TestFixture]
    public class UpdateFixture : CoreTest<HttpApiProvider>
    {
        private XbmcSettings _settings;
        private string _artistQueryUrl = "http://localhost:8080/xbmcCmds/xbmcHttp?command=QueryMusicDatabase(select path.strPath from path, artist, artistlinkpath where artist.c12 = 9f4e41c3-2648-428e-b8c7-dc10465b49ac and artistlinkpath.idArtist = artist.idArtist and artistlinkpath.idPath = path.idPath)";
        private Artist _fakeArtist;

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

            _fakeArtist = Builder<Artist>.CreateNew()
                                         .With(s => s.ForeignArtistId = "9f4e41c3-2648-428e-b8c7-dc10465b49ac")
                                         .With(s => s.Name = "Shawn Desman")
                                         .Build();
        }

        private void WithArtistPath()
        {
            Mocker.GetMock<IHttpProvider>()
                  .Setup(s => s.DownloadString(_artistQueryUrl, _settings.Username, _settings.Password))
                  .Returns("<xml><record><field>smb://xbmc:xbmc@HOMESERVER/Music/Shawn Desman/</field></record></xml>");
        }

        private void WithoutArtistPath()
        {
            Mocker.GetMock<IHttpProvider>()
                  .Setup(s => s.DownloadString(_artistQueryUrl, _settings.Username, _settings.Password))
                  .Returns("<xml></xml>");
        }

        [Test]
        public void should_update_using_artist_path()
        {
            WithArtistPath();
            const string url = "http://localhost:8080/xbmcCmds/xbmcHttp?command=ExecBuiltIn(UpdateLibrary(music,smb://xbmc:xbmc@HOMESERVER/Music/Shawn Desman/))";

            Mocker.GetMock<IHttpProvider>().Setup(s => s.DownloadString(url, _settings.Username, _settings.Password));

            Subject.Update(_settings, _fakeArtist);
            Mocker.VerifyAllMocks();
        }

        [Test]
        public void should_update_all_paths_when_artist_path_not_found()
        {
            WithoutArtistPath();
            const string url = "http://localhost:8080/xbmcCmds/xbmcHttp?command=ExecBuiltIn(UpdateLibrary(music))";

            Mocker.GetMock<IHttpProvider>().Setup(s => s.DownloadString(url, _settings.Username, _settings.Password));

            Subject.Update(_settings, _fakeArtist);
            Mocker.VerifyAllMocks();
        }
    }
}
