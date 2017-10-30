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
    public class GetSeriesPathFixture : CoreTest<JsonApiProvider>
    {
        private const string MB_ID = "5";
        private XbmcSettings _settings;
        private Artist _artist;
        private List<TvShow> _xbmcSeries;

        [SetUp]
        public void Setup()
        {
            _settings = Builder<XbmcSettings>.CreateNew()
                                             .Build();

            _xbmcSeries = Builder<TvShow>.CreateListOfSize(3)
                                         .All()
                                         .With(s => s.ImdbNumber = "0")
                                         .TheFirst(1)
                                         .With(s => s.ImdbNumber = MB_ID.ToString())
                                         .Build()
                                         .ToList();

            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Setup(s => s.GetArtist(_settings))
                  .Returns(_xbmcSeries);
        }

        private void GivenMatchingTvdbId()
        {
            _artist = new Artist
            {
                              ForeignArtistId = MB_ID,
                              Name = "TV Show"
                          };
        }

        private void GivenMatchingTitle()
        {
            _artist = new Artist
            {
                ForeignArtistId = "1000",
                Name = _xbmcSeries.First().Label
            };
        }

        private void GivenMatchingSeries()
        {
            _artist = new Artist
            {
                ForeignArtistId = "1000",
                Name = "Does not exist"
            }; 
        }

        [Test]
        public void should_return_null_when_series_is_not_found()
        {
            GivenMatchingSeries();

            Subject.GetSeriesPath(_settings, _artist).Should().BeNull();
        }

        [Test]
        public void should_return_path_when_tvdbId_matches()
        {
            GivenMatchingTvdbId();

            Subject.GetSeriesPath(_settings, _artist).Should().Be(_xbmcSeries.First().File);
        }

        [Test]
        public void should_return_path_when_title_matches()
        {
            GivenMatchingTitle();

            Subject.GetSeriesPath(_settings, _artist).Should().Be(_xbmcSeries.First().File);
        }

        [Test]
        public void should_not_throw_when_imdb_number_is_not_a_number()
        {
            GivenMatchingTvdbId();

            _xbmcSeries.ForEach(s => s.ImdbNumber = "tt12345");
            _xbmcSeries.Last().ImdbNumber = MB_ID.ToString();

            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Setup(s => s.GetArtist(_settings))
                  .Returns(_xbmcSeries);

            Subject.GetSeriesPath(_settings, _artist).Should().NotBeNull();
        }
    }
}
