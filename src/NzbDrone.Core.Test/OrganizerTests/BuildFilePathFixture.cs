using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.OrganizerTests
{
    [TestFixture]
    [Ignore("Don't use album folder in readarr")]
    public class BuildFilePathFixture : CoreTest<FileNameBuilder>
    {
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _namingConfig = NamingConfig.Default;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);
        }

        [Test]
        public void should_clean_album_folder_when_it_contains_illegal_characters_in_album_or_artist_title()
        {
            var filename = @"bookfile";
            var expectedPath = @"C:\Test\Fake- The Author\Fake- The Book\bookfile.mobi";

            var fakeArtist = Builder<Author>.CreateNew()
                .With(s => s.Name = "Fake: The Author")
                .With(s => s.Path = @"C:\Test\Fake- The Author".AsOsAgnostic())
                .Build();

            var fakeAlbum = Builder<Book>.CreateNew()
                .With(s => s.Title = "Fake: Book")
                .Build();

            Subject.BuildBookFilePath(fakeArtist, fakeAlbum, filename, ".mobi").Should().Be(expectedPath.AsOsAgnostic());
        }
    }
}
