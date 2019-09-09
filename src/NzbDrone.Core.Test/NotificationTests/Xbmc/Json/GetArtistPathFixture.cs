using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Notifications.Xbmc;
using NzbDrone.Core.Notifications.Xbmc.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Test.NotificationTests.Xbmc.Json
{
    [TestFixture]
    public class GetArtistPathFixture : CoreTest<JsonApiProvider>
    {
        private const string MB_ID = "9f4e41c3-2648-428e-b8c7-dc10465b49ac";
        private XbmcSettings _settings;
        private Music.Artist _artist;
        private List<KodiArtist> _xbmcArtist;

        [SetUp]
        public void Setup()
        {
            _settings = Builder<XbmcSettings>.CreateNew()
                                             .Build();

            _xbmcArtist = Builder<KodiArtist>.CreateListOfSize(3)
                                         .All()
                                         .With(s => s.MusicbrainzArtistId = new List<string>{"0"})
                                         .TheFirst(1)
                                         .With(s => s.MusicbrainzArtistId = new List<string> {MB_ID.ToString()})
                                         .Build()
                                         .ToList();

            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Setup(s => s.GetArtist(_settings))
                  .Returns(_xbmcArtist);
        }

        private void GivenMatchingMusicbrainzId()
        {
            _artist = new Artist
            {
                              ForeignArtistId = MB_ID,
                              Name = "Artist"
                          };
        }

        private void GivenMatchingTitle()
        {
            _artist = new Artist
            {
                ForeignArtistId = "1000",
                Name = _xbmcArtist.First().Label
            };
        }

        private void GivenMatchingArtist()
        {
            _artist = new Artist
            {
                ForeignArtistId = "1000",
                Name = "Does not exist"
            }; 
        }

        [Test]
        public void should_return_null_when_artist_is_not_found()
        {
            GivenMatchingArtist();

            Subject.GetArtistPath(_settings, _artist).Should().BeNull();
        }

        [Test]
        public void should_return_path_when_musicbrainzId_matches()
        {
            GivenMatchingMusicbrainzId();

            Subject.GetArtistPath(_settings, _artist).Should().Be(_xbmcArtist.First().File);
        }

        [Test]
        public void should_return_path_when_title_matches()
        {
            GivenMatchingTitle();

            Subject.GetArtistPath(_settings, _artist).Should().Be(_xbmcArtist.First().File);
        }
    }
}
