using System.Linq;
using FluentAssertions;
using NLog;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Music;
using NzbDrone.Core.Test.Framework;
using System.Collections.Generic;

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

            var trackNames = new List<string> {
                "Courage",
                "Movies",
                "Flesh and Bone",
                "Whisper",
                "Summer",
                "Sticks and Stones",
                "Attitude",
                "Stranded",
                "Wish",
                "Calico",
                "(Happy) Death Day",
                "Smooth Criminal",
                "Universe / Orange Appeal"
            };

            for (int i = 0; i < trackNames.Count; i++) {
                _trackRepository.Insert(new Track
                        {
                            Title = trackNames[i],
                            ForeignTrackId = (i+1).ToString(),
                            AlbumId = 4321,
                            AbsoluteTrackNumber = i+1,
                            MediumNumber = 1,
                            TrackFileId = i+1
                        });
            }
        }

        [Test]
        public void should_find_track_in_db_by_tracktitle_longer_then_releasetitle()
        {
            var track = _trackService.FindTrackByTitle(1234, 4321, 1, 1, "Courage with some bla");

            track.Should().NotBeNull();
            track.Title.Should().Be(_trackRepository.GetTracksByFileId(1).First().Title);
        }

        [Test]
        public void should_find_track_in_db_by_tracktitle_shorter_then_releasetitle()
        {
            var track = _trackService.FindTrackByTitle(1234, 4321, 1, 3, "and Bone");

            track.Should().NotBeNull();
            track.Title.Should().Be(_trackRepository.GetTracksByFileId(3).First().Title);
        }

        [Test]
        public void should_not_find_track_in_db_by_wrong_title()
        {
            var track = _trackService.FindTrackByTitle(1234, 4321, 1, 1, "Not a track");

            track.Should().BeNull();
        }

        [TestCase("Fesh and Bone", 3)]
        [TestCase("Atitude", 7)]
        [TestCase("Smoth cRimnal", 12)]
        [TestCase("Sticks and Stones (live)", 6)]
        public void should_find_track_in_db_by_inexact_title(string title, int trackNumber)
        {
            var track = _trackService.FindTrackByTitleInexact(1234, 4321, 1, trackNumber, title);

            track.Should().NotBeNull();
            track.Title.Should().Be(_trackRepository.GetTracksByFileId(trackNumber).First().Title);
        }

        [TestCase("A random title", 1)]
        [TestCase("Stones and Sticks", 6)]
        public void should_not_find_track_in_db_by_different_inexact_title(string title, int trackId)
        {
            var track = _trackService.FindTrackByTitleInexact(1234, 4321, 1, trackId, title);

            track.Should().BeNull();
        }


    }
}
