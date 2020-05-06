using System.Collections.Generic;
using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Music;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MusicTests
{
    [TestFixture]
    public class AddArtistFixture : CoreTest<AddArtistService>
    {
        private Author _fakeArtist;

        [SetUp]
        public void Setup()
        {
            _fakeArtist = Builder<Author>
                .CreateNew()
                .With(s => s.Path = null)
                .Build();
            _fakeArtist.Books = new List<Book>();
        }

        private void GivenValidArtist(string readarrId)
        {
            Mocker.GetMock<IProvideAuthorInfo>()
                .Setup(s => s.GetAuthorInfo(readarrId))
                .Returns(_fakeArtist);
        }

        private void GivenValidPath()
        {
            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.GetArtistFolder(It.IsAny<Author>(), null))
                  .Returns<Author, NamingConfig>((c, n) => c.Name);

            Mocker.GetMock<IAddArtistValidator>()
                  .Setup(s => s.Validate(It.IsAny<Author>()))
                  .Returns(new ValidationResult());
        }

        [Test]
        public void should_be_able_to_add_a_artist_without_passing_in_name()
        {
            var newArtist = new Author
            {
                ForeignAuthorId = "ce09ea31-3d4a-4487-a797-e315175457a0",
                RootFolderPath = @"C:\Test\Music"
            };

            GivenValidArtist(newArtist.ForeignAuthorId);
            GivenValidPath();

            var artist = Subject.AddArtist(newArtist);

            artist.Name.Should().Be(_fakeArtist.Name);
        }

        [Test]
        public void should_have_proper_path()
        {
            var newArtist = new Author
            {
                ForeignAuthorId = "ce09ea31-3d4a-4487-a797-e315175457a0",
                RootFolderPath = @"C:\Test\Music"
            };

            GivenValidArtist(newArtist.ForeignAuthorId);
            GivenValidPath();

            var artist = Subject.AddArtist(newArtist);

            artist.Path.Should().Be(Path.Combine(newArtist.RootFolderPath, _fakeArtist.Name));
        }

        [Test]
        public void should_throw_if_artist_validation_fails()
        {
            var newArtist = new Author
            {
                ForeignAuthorId = "ce09ea31-3d4a-4487-a797-e315175457a0",
                Path = @"C:\Test\Music\Name1"
            };

            GivenValidArtist(newArtist.ForeignAuthorId);

            Mocker.GetMock<IAddArtistValidator>()
                  .Setup(s => s.Validate(It.IsAny<Author>()))
                  .Returns(new ValidationResult(new List<ValidationFailure>
                                                {
                                                    new ValidationFailure("Path", "Test validation failure")
                                                }));

            Assert.Throws<ValidationException>(() => Subject.AddArtist(newArtist));
        }

        [Test]
        public void should_throw_if_artist_cannot_be_found()
        {
            var newArtist = new Author
            {
                ForeignAuthorId = "ce09ea31-3d4a-4487-a797-e315175457a0",
                Path = @"C:\Test\Music\Name1"
            };

            Mocker.GetMock<IProvideAuthorInfo>()
                  .Setup(s => s.GetAuthorInfo(newArtist.ForeignAuthorId))
                  .Throws(new ArtistNotFoundException(newArtist.ForeignAuthorId));

            Mocker.GetMock<IAddArtistValidator>()
                  .Setup(s => s.Validate(It.IsAny<Author>()))
                  .Returns(new ValidationResult(new List<ValidationFailure>
                                                {
                                                    new ValidationFailure("Path", "Test validation failure")
                                                }));

            Assert.Throws<ValidationException>(() => Subject.AddArtist(newArtist));

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_disambiguate_if_artist_folder_exists()
        {
            var newArtist = new Author
            {
                ForeignAuthorId = "ce09ea31-3d4a-4487-a797-e315175457a0",
                Path = @"C:\Test\Music\Name1",
            };

            _fakeArtist.Metadata = Builder<AuthorMetadata>.CreateNew().With(x => x.Disambiguation = "Disambiguation").Build();

            GivenValidArtist(newArtist.ForeignAuthorId);
            GivenValidPath();

            Mocker.GetMock<IArtistService>()
                .Setup(x => x.ArtistPathExists(newArtist.Path))
                .Returns(true);

            var artist = Subject.AddArtist(newArtist);
            artist.Path.Should().Be(newArtist.Path + " (Disambiguation)");
        }

        [Test]
        public void should_disambiguate_with_numbers_if_artist_folder_still_exists()
        {
            var newArtist = new Author
            {
                ForeignAuthorId = "ce09ea31-3d4a-4487-a797-e315175457a0",
                Path = @"C:\Test\Music\Name1",
            };

            _fakeArtist.Metadata = Builder<AuthorMetadata>.CreateNew().With(x => x.Disambiguation = "Disambiguation").Build();

            GivenValidArtist(newArtist.ForeignAuthorId);
            GivenValidPath();

            Mocker.GetMock<IArtistService>()
                .Setup(x => x.ArtistPathExists(newArtist.Path))
                .Returns(true);

            Mocker.GetMock<IArtistService>()
                .Setup(x => x.ArtistPathExists(newArtist.Path + " (Disambiguation)"))
                .Returns(true);

            Mocker.GetMock<IArtistService>()
                .Setup(x => x.ArtistPathExists(newArtist.Path + " (Disambiguation) (1)"))
                .Returns(true);

            Mocker.GetMock<IArtistService>()
                .Setup(x => x.ArtistPathExists(newArtist.Path + " (Disambiguation) (2)"))
                .Returns(true);

            var artist = Subject.AddArtist(newArtist);
            artist.Path.Should().Be(newArtist.Path + " (Disambiguation) (3)");
        }

        [Test]
        public void should_disambiguate_with_numbers_if_artist_folder_exists_and_no_disambiguation()
        {
            var newArtist = new Author
            {
                ForeignAuthorId = "ce09ea31-3d4a-4487-a797-e315175457a0",
                Path = @"C:\Test\Music\Name1",
            };

            _fakeArtist.Metadata = Builder<AuthorMetadata>.CreateNew().With(x => x.Disambiguation = string.Empty).Build();

            GivenValidArtist(newArtist.ForeignAuthorId);
            GivenValidPath();

            Mocker.GetMock<IArtistService>()
                .Setup(x => x.ArtistPathExists(newArtist.Path))
                .Returns(true);

            Mocker.GetMock<IArtistService>()
                .Setup(x => x.ArtistPathExists(newArtist.Path + " (1)"))
                .Returns(true);

            Mocker.GetMock<IArtistService>()
                .Setup(x => x.ArtistPathExists(newArtist.Path + " (2)"))
                .Returns(true);

            var artist = Subject.AddArtist(newArtist);
            artist.Path.Should().Be(newArtist.Path + " (3)");
        }
    }
}
