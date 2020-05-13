using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.OrganizerTests
{
    [TestFixture]

    public class GetAuthorFolderFixture : CoreTest<FileNameBuilder>
    {
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _namingConfig = NamingConfig.Default;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);
        }

        [TestCase("Avenged Sevenfold", "{Author Name}", "Avenged Sevenfold")]
        [TestCase("Avenged Sevenfold", "{Author.Name}", "Avenged.Sevenfold")]
        [TestCase("AC/DC", "{Author Name}", "AC+DC")]
        [TestCase("In the Woods...", "{Author.Name}", "In.the.Woods")]
        [TestCase("3OH!3", "{Author.Name}", "3OH!3")]
        [TestCase("Avenged Sevenfold", ".{Author.Name}.", "Avenged.Sevenfold")]
        public void should_use_authorFolderFormat_to_build_folder_name(string authorName, string format, string expected)
        {
            _namingConfig.AuthorFolderFormat = format;

            var author = new Author { Name = authorName };

            Subject.GetAuthorFolder(author).Should().Be(expected);
        }
    }
}
