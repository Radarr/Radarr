using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Music;
using NzbDrone.Core.Test.Framework;
using System.Collections.Generic;
using Moq;

namespace NzbDrone.Core.Test.MusicTests.TitleMatchingTests
{
    [TestFixture]
    public class TitleMatchingFixture : CoreTest<TrackService>
    {
        private List<Track> _tracks;
        
        [SetUp]
        public void Setup()
        {
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
                "Universe / Orange Appeal",
                "Christian's Inferno"
            };

            _tracks = new List<Track>();
            for (int i = 0; i < trackNames.Count; i++) {
                _tracks.Add(new Track
                    {
                        Title = trackNames[i],
                        ForeignTrackId = (i+1).ToString(),
                        AbsoluteTrackNumber = i+1,
                        MediumNumber = 1
                    });
            }

            Mocker.GetMock<ITrackRepository>()
                .Setup(s => s.GetTracksByMedium(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(_tracks);

            Mocker.GetMock<ITrackRepository>()
                .Setup(s => s.Find(1234, 4321, It.IsAny<int>(), It.IsAny<int>()))
                .Returns((int artistid, int albumid, int medium, int track) => _tracks.Where(t => t.AbsoluteTrackNumber == track && t.MediumNumber == medium).Single());
        }

        private void GivenSecondDisc()
        {
            var trackNames = new List<string> {
                "Courage",
                "another entry",
                "random name"
            };

            for (int i = 0; i < trackNames.Count; i++) {
                _tracks.Add(new Track
                    {
                        Title = trackNames[i],
                        ForeignTrackId = (100+i+1).ToString(),
                        AbsoluteTrackNumber = i+1,
                        MediumNumber = 2
                    });
            }
        }

        [Test]
        public void should_find_track_in_db_by_tracktitle_longer_then_releasetitle()
        {
            var track = Subject.FindTrackByTitle(1234, 4321, 1, 1, "Courage with some bla");

            track.Should().NotBeNull();
            track.Title.Should().Be(Subject.FindTrack(1234, 4321, 1, 1).Title);
        }

        [Test]
        public void should_find_track_in_db_by_tracktitle_shorter_then_releasetitle()
        {
            var track = Subject.FindTrackByTitle(1234, 4321, 1, 3, "and Bone");

            track.Should().NotBeNull();
            track.Title.Should().Be(Subject.FindTrack(1234, 4321, 1, 3).Title);
        }

        [Test]
        public void should_not_find_track_in_db_by_wrong_title()
        {
            var track = Subject.FindTrackByTitle(1234, 4321, 1, 1, "Not a track");

            track.Should().BeNull();
        }

        [TestCase("another entry", 2, 2)]
        [TestCase("random name", 2, 3)]
        public void should_find_track_on_second_disc_when_disc_tag_missing(string title, int discNumber, int trackNumber)
        {
            GivenSecondDisc();
            var track = Subject.FindTrackByTitle(1234, 4321, 0, trackNumber, title);
            var expected = Subject.FindTrack(1234, 4321, discNumber, trackNumber);

            track.Should().NotBeNull();
            expected.Should().NotBeNull();

            track.Title.Should().Be(expected.Title);
        }

        [Test]
        public void should_return_null_if_tracks_with_same_name_and_number_on_different_discs()
        {
            GivenSecondDisc();
            var track = Subject.FindTrackByTitle(1234, 4321, 0, 1, "Courage");
            track.Should().BeNull();
        }

        [TestCase("Fesh and Bone", 3)]
        [TestCase("Atitude", 7)]
        [TestCase("Smoth cRimnal", 12)]
        [TestCase("Sticks and Stones (live)", 6)]
        [TestCase("Sticks and Stones (live) - there's a lot of rubbish here", 6)]
        [TestCase("Smoth cRimnal feat. someone I don't care about", 12)]
        [TestCase("Christians Inferno", 14)]
        [TestCase("xxxyyy some random prefix Christians Infurno", 14)]
        public void should_find_track_in_db_by_inexact_title(string title, int trackNumber)
        {
            var track = Subject.FindTrackByTitleInexact(1234, 4321, 1, trackNumber, title);
            var expected = Subject.FindTrack(1234, 4321, 1, trackNumber);

            track.Should().NotBeNull();
            expected.Should().NotBeNull();

            track.Title.Should().Be(expected.Title);
        }

        [TestCase("Fesh and Bone", 1)]
        [TestCase("Atitude", 1)]
        [TestCase("Smoth cRimnal", 1)]
        [TestCase("Sticks and Stones (live)", 1)]
        [TestCase("Christians Inferno", 1)]
        public void should_not_find_track_in_db_by_inexact_title_with_wrong_tracknumber(string title, int trackNumber)
        {
            var track = Subject.FindTrackByTitleInexact(1234, 4321, 1, trackNumber, title);

            track.Should().BeNull();
        }

        [TestCase("Movis", 1, 2)]
        [TestCase("anoth entry", 2, 2)]
        [TestCase("random.name", 2, 3)]
        public void should_find_track_in_db_by_inexact_title_when_disc_tag_missing(string title, int discNumber, int trackNumber)
        {
            GivenSecondDisc();
            var track = Subject.FindTrackByTitleInexact(1234, 4321, 0, trackNumber, title);
            var expected = Subject.FindTrack(1234, 4321, discNumber, trackNumber);

            track.Should().NotBeNull();
            expected.Should().NotBeNull();

            track.Title.Should().Be(expected.Title);
        }

        [TestCase("A random title", 1)]
        [TestCase("Stones and Sticks", 6)]
        public void should_not_find_track_in_db_by_different_inexact_title(string title, int trackId)
        {
            var track = Subject.FindTrackByTitleInexact(1234, 4321, 1, trackId, title);

            track.Should().BeNull();
        }


    }
}
