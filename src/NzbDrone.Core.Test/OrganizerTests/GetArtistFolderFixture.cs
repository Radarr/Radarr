using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Music;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.OrganizerTests
{
    [TestFixture]

    public class GetArtistFolderFixture : CoreTest<FileNameBuilder>
    {
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _namingConfig = NamingConfig.Default;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);
        }

        [TestCase("Avenged Sevenfold", "{Artist Name}", "Avenged Sevenfold")]
        [TestCase("Avenged Sevenfold", "{Artist.Name}", "Avenged.Sevenfold")]
        [TestCase("AC/DC", "{Artist Name}", "AC+DC")]
        [TestCase("In the Woods...", "{Artist.Name}", "In.the.Woods")]
        [TestCase("3OH!3", "{Artist.Name}", "3OH!3")]
        [TestCase("Avenged Sevenfold", ".{Artist.Name}.", "Avenged.Sevenfold")]
        public void should_use_artistFolderFormat_to_build_folder_name(string artistName, string format, string expected)
        {
            _namingConfig.ArtistFolderFormat = format;

            var artist = new Author { Name = artistName };

            Subject.GetArtistFolder(artist).Should().Be(expected);
        }
    }
}
