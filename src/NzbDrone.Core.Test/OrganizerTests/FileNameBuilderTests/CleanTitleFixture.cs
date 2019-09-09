using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [TestFixture]
    public class CleanTitleFixture : CoreTest<FileNameBuilder>
    {
        private Artist _artist;
        private Album _album;
        private AlbumRelease _release;
        private Track _track;
        private TrackFile _trackFile;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _artist = Builder<Artist>
                    .CreateNew()
                    .With(s => s.Name = "Avenged Sevenfold")
                    .Build();

            _album = Builder<Album>
                    .CreateNew()
                    .With(s => s.Title = "Hail to the King")
                    .Build();

            _release = Builder<AlbumRelease>
                .CreateNew()
                .With(s => s.Media = new List<Medium> { new Medium { Number = 1 } })
                .Build();

            _track = Builder<Track>.CreateNew()
                            .With(e => e.Title = "Doing Time")
                            .With(e => e.AbsoluteTrackNumber = 3)
                            .With(e => e.AlbumRelease = _release)
                            .Build();

            _trackFile = new TrackFile { Quality = new QualityModel(Quality.MP3_256), ReleaseGroup = "LidarrTest" };

            _namingConfig = NamingConfig.Default;
            _namingConfig.RenameTracks = true;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(Moq.It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));
        }

        [TestCase("Florence + the Machine", "Florence + the Machine")]
        [TestCase("Beyoncé X10", "Beyoncé X10")]
        [TestCase("Girlfriends' Guide to Divorce", "Girlfriends Guide to Divorce")]
        [TestCase("Rule #23: Never Lie to the Kids", "Rule #23 Never Lie to the Kids")]
        [TestCase("Anne Hathaway/Florence + The Machine", "Anne Hathaway Florence + The Machine")]
        [TestCase("Chris Rock/Prince", "Chris Rock Prince")]
        [TestCase("Karma's a B*tch!", "Karmas a B-tch!")]
        [TestCase("Ke$ha: My Crazy Beautiful Life", "Ke$ha My Crazy Beautiful Life")]
        [TestCase("$#*! My Dad Says", "$#-! My Dad Says")]
        [TestCase("Free! - Iwatobi Swim Club", "Free! Iwatobi Swim Club")]
        [TestCase("Tamara Ecclestone: Billion $$ Girl", "Tamara Ecclestone Billion $$ Girl")]
        [TestCase("Marvel's Agents of S.H.I.E.L.D.", "Marvels Agents of S.H.I.E.L.D")]
        [TestCase("Castle (2009)", "Castle 2009")]
        [TestCase("Law & Order (UK)", "Law and Order UK")]
        [TestCase("Is this okay?", "Is this okay")]
        [TestCase("[a] title", "a title")]
        [TestCase("backslash \\ backlash", "backslash backlash")]
        [TestCase("I'm the Boss", "Im the Boss")]
        public void should_get_expected_title_back(string name, string expected)
        {
            _artist.Name = name;
            _namingConfig.StandardTrackFormat = "{Artist CleanName}";

            Subject.BuildTrackFileName(new List<Track> { _track }, _artist, _album, _trackFile)
                   .Should().Be(expected);
        }

        [Test]
        public void should_use_and_as_separator_for_multiple_episodes()
        {
            var tracks = Builder<Track>.CreateListOfSize(2)
                                           .TheFirst(1)
                                           .With(e => e.Title = "Surrender Benson")
                                           .TheNext(1)
                                           .With(e => e.Title = "Imprisoned Lives")
                                           .All()
                                           .With(e => e.AlbumRelease = _release)
                                           .Build()
                                           .ToList();

            _namingConfig.StandardTrackFormat = "{Track CleanTitle}";

            Subject.BuildTrackFileName(tracks, _artist, _album, _trackFile)
                   .Should().Be(tracks.First().Title + " and " + tracks.Last().Title);
        }
    }
}
