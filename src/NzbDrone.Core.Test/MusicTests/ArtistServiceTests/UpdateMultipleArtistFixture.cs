using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Music;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MusicTests.ArtistServiceTests
{
    [TestFixture]
    public class UpdateMultipleArtistFixture : CoreTest<ArtistService>
    {
        private List<Artist> _artists;

        [SetUp]
        public void Setup()
        {
            _artists = Builder<Artist>.CreateListOfSize(5)
                .All()
                .With(s => s.QualityProfileId = 1)
                .With(s => s.Monitored)
                .With(s => s.Path = @"C:\Test\name".AsOsAgnostic())
                .With(s => s.RootFolderPath = "")
                .Build().ToList();
        }

        [Test]
        public void should_call_repo_updateMany()
        {
            Subject.UpdateArtists(_artists, false);

            Mocker.GetMock<IArtistRepository>().Verify(v => v.UpdateMany(_artists), Times.Once());
        }

        [Test]
        public void should_update_path_when_rootFolderPath_is_supplied()
        {
            Mocker.GetMock<IBuildFileNames>()
                .Setup(s => s.GetArtistFolder(It.IsAny<Artist>(), null))
                .Returns<Artist, NamingConfig>((c, n) => c.Name);

            var newRoot = @"C:\Test\Music2".AsOsAgnostic();
            _artists.ForEach(s => s.RootFolderPath = newRoot);

            Mocker.GetMock<IBuildArtistPaths>()
                .Setup(s => s.BuildPath(It.IsAny<Artist>(), false))
                .Returns<Artist, bool>((s, u) => Path.Combine(s.RootFolderPath, s.Name));


            Subject.UpdateArtists(_artists, false).ForEach(s => s.Path.Should().StartWith(newRoot));
        }

        [Test]
        public void should_not_update_path_when_rootFolderPath_is_empty()
        {
            Subject.UpdateArtists(_artists, false).ForEach(s =>
            {
                var expectedPath = _artists.Single(ser => ser.Id == s.Id).Path;
                s.Path.Should().Be(expectedPath);
            });
        }

        [Test]
        public void should_be_able_to_update_many_artist()
        {
            var artist = Builder<Artist>.CreateListOfSize(50)
                                        .All()
                                        .With(s => s.Path = (@"C:\Test\Music\" + s.Path).AsOsAgnostic())
                                        .Build()
                                        .ToList();

            Mocker.GetMock<IBuildFileNames>()
                .Setup(s => s.GetArtistFolder(It.IsAny<Artist>(), null))
                .Returns<Artist, NamingConfig>((c, n) => c.Name);

            var newRoot = @"C:\Test\Music2".AsOsAgnostic();
            artist.ForEach(s => s.RootFolderPath = newRoot);

            Subject.UpdateArtists(artist, false);
        }
    }
}
