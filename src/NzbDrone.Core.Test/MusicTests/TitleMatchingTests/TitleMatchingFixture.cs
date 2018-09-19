using System.Linq;
using FluentAssertions;
using NLog;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Music;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MusicTests.TitleMatchingTests
{
    [TestFixture]
    public class TitleMatchingFixture : DbTest<TrackService, Track>
    {
        private TrackRepository _trackRepository;
        private TrackService _trackService;

        [SetUp]
        public void Setup()
        {
            _trackRepository = Mocker.Resolve<TrackRepository>();
            _trackService =
                new TrackService(_trackRepository, Mocker.Resolve<ConfigService>(), Mocker.Resolve<Logger>());

            _trackRepository.Insert(new Track
            {
                Title = "This is the short test title",
                ForeignTrackId = "this is a fake id2",
                AlbumId = 4321,
                AbsoluteTrackNumber = 1,
                MediumNumber = 1,
                TrackFileId = 1
            });

            _trackRepository.Insert(new Track
            {
                Title = "This is the long test title",
                ForeignTrackId = "this is a fake id",
                AlbumId = 4321,
                AbsoluteTrackNumber = 2,
                MediumNumber = 1,
                TrackFileId = 2
            });
        }

        [Test]
        public void should_find_track_in_db_by_tracktitle_longer_then_relaeasetitle()
        {
            var track = _trackService.FindTrackByTitle(1234, 4321, 1, 1, "This is the short test title with some bla");

            track.Title.Should().Be(_trackRepository.GetTracksByFileId(1).First().Title);
        }

        [Test]
        public void should_find_track_in_db_by_tracktitle_shorter_then_relaeasetitle()
        {
            var track = _trackService.FindTrackByTitle(1234, 4321, 1, 2, "test title");

            track.Title.Should().Be(_trackRepository.GetTracksByFileId(2).First().Title);
        }

        [Test]
        public void should_not_find_track_in_db_by_wrong_title()
        {
            var track = _trackService.FindTrackByTitle(1234, 4321, 1, 1, "the short title");

            track.Should().BeNull();
        }
    }
}
