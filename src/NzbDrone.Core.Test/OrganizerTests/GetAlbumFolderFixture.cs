using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Test.OrganizerTests
{
    [TestFixture]
    public class GetAlbumFolderFixture : CoreTest<FileNameBuilder>
    {
        private NamingConfig namingConfig;

        [SetUp]
        public void Setup()
        {
            namingConfig = NamingConfig.Default;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(namingConfig);
        }

        [TestCase("Venture Bros.", "Today", "{Artist.Name}.{Album.Title}", "Venture.Bros.Today")]
        [TestCase("Venture Bros.", "Today", "{Artist Name} {Album Title}", "Venture Bros. Today")]
        public void should_use_albumFolderFormat_to_build_folder_name(string artistName, string albumTitle, string format, string expected)
        {
            namingConfig.AlbumFolderFormat = format;

            var artist = new Artist { Name = artistName };
            var album = new Album { Title = albumTitle };

            Subject.GetAlbumFolder(artist, album, namingConfig).Should().Be(expected);
        }
    }
}