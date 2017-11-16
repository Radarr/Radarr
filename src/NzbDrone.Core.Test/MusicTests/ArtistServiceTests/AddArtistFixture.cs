using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Music;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music.Events;

namespace NzbDrone.Core.Test.MusicTests.ArtistServiceTests
{
    [TestFixture]
    public class AddArtistFixture : CoreTest<ArtistService>
    {
        private Artist _fakeArtist;

        [SetUp]
        public void Setup()
        {
            _fakeArtist = Builder<Artist>.CreateNew().Build();
        }

        [Test]
        public void artist_added_event_should_have_proper_path()
        {
            _fakeArtist.Path = null;
            _fakeArtist.RootFolderPath = @"C:\Test\Music";

            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.GetArtistFolder(_fakeArtist, null))
                  .Returns(_fakeArtist.Name);

            var artist = Subject.AddArtist(_fakeArtist);

            artist.Path.Should().NotBeNull();

            VerifyEventPublished<ArtistAddedEvent>();
        }

    }
}
