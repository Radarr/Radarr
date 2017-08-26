using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Test.OrganizerTests
{
    [TestFixture]

    public class GetArtistFolderFixture : CoreTest<FileNameBuilder>
    {
        private NamingConfig namingConfig;

        [SetUp]
        public void Setup()
        {
            namingConfig = NamingConfig.Default;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(namingConfig);
        }

        [TestCase("Avenged Sevenfold", "{Artist Name}", "Avenged Sevenfold")]
        [TestCase("Avenged Sevenfold", "{Artist.Name}", "Avenged.Sevenfold")]
        [TestCase("AC/DC", "{Artist Name}", "AC+DC")]
        [TestCase("In the Woods...", "{Artist.Name}", "In.the.Woods")]
        [TestCase("3OH!3", "{Artist.Name}", "3OH!3")]
        [TestCase("Avenged Sevenfold", ".{Artist.Name}.", "Avenged.Sevenfold")]
        public void should_use_artistFolderFormat_to_build_folder_name(string artistName, string format, string expected)
        {
            namingConfig.ArtistFolderFormat = format;

            var artist = new Artist { Name = artistName };

            Subject.GetArtistFolder(artist).Should().Be(expected);
        }
    }
}