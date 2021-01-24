using System.Collections.Generic;
using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MusicTests
{
    [TestFixture]
    public class AddAuthorFixture : CoreTest<AddAuthorService>
    {
        private Author _fakeAuthor;

        [SetUp]
        public void Setup()
        {
            _fakeAuthor = Builder<Author>
                .CreateNew()
                .With(s => s.Path = null)
                .Build();
            _fakeAuthor.Books = new List<Book>();
        }

        private void GivenValidAuthor(string readarrId)
        {
            Mocker.GetMock<IProvideAuthorInfo>()
                .Setup(s => s.GetAuthorInfo(readarrId, true))
                .Returns(_fakeAuthor);
        }

        private void GivenValidPath()
        {
            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.GetAuthorFolder(It.IsAny<Author>(), null))
                  .Returns<Author, NamingConfig>((c, n) => c.Name);

            Mocker.GetMock<IAddAuthorValidator>()
                  .Setup(s => s.Validate(It.IsAny<Author>()))
                  .Returns(new ValidationResult());
        }

        [Test]
        public void should_be_able_to_add_a_author_without_passing_in_name()
        {
            var newAuthor = new Author
            {
                ForeignAuthorId = "ce09ea31-3d4a-4487-a797-e315175457a0",
                RootFolderPath = @"C:\Test\Music"
            };

            GivenValidAuthor(newAuthor.ForeignAuthorId);
            GivenValidPath();

            var author = Subject.AddAuthor(newAuthor);

            author.Name.Should().Be(_fakeAuthor.Name);
        }

        [Test]
        public void should_have_proper_path()
        {
            var newAuthor = new Author
            {
                ForeignAuthorId = "ce09ea31-3d4a-4487-a797-e315175457a0",
                RootFolderPath = @"C:\Test\Music"
            };

            GivenValidAuthor(newAuthor.ForeignAuthorId);
            GivenValidPath();

            var author = Subject.AddAuthor(newAuthor);

            author.Path.Should().Be(Path.Combine(newAuthor.RootFolderPath, _fakeAuthor.Name));
        }

        [Test]
        public void should_throw_if_author_validation_fails()
        {
            var newAuthor = new Author
            {
                ForeignAuthorId = "ce09ea31-3d4a-4487-a797-e315175457a0",
                Path = @"C:\Test\Music\Name1"
            };

            GivenValidAuthor(newAuthor.ForeignAuthorId);

            Mocker.GetMock<IAddAuthorValidator>()
                  .Setup(s => s.Validate(It.IsAny<Author>()))
                  .Returns(new ValidationResult(new List<ValidationFailure>
                                                {
                                                    new ValidationFailure("Path", "Test validation failure")
                                                }));

            Assert.Throws<ValidationException>(() => Subject.AddAuthor(newAuthor));
        }

        [Test]
        public void should_throw_if_author_cannot_be_found()
        {
            var newAuthor = new Author
            {
                ForeignAuthorId = "ce09ea31-3d4a-4487-a797-e315175457a0",
                Path = @"C:\Test\Music\Name1"
            };

            Mocker.GetMock<IProvideAuthorInfo>()
                  .Setup(s => s.GetAuthorInfo(newAuthor.ForeignAuthorId, true))
                  .Throws(new AuthorNotFoundException(newAuthor.ForeignAuthorId));

            Mocker.GetMock<IAddAuthorValidator>()
                  .Setup(s => s.Validate(It.IsAny<Author>()))
                  .Returns(new ValidationResult(new List<ValidationFailure>
                                                {
                                                    new ValidationFailure("Path", "Test validation failure")
                                                }));

            Assert.Throws<ValidationException>(() => Subject.AddAuthor(newAuthor));

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_disambiguate_if_author_folder_exists()
        {
            var newAuthor = new Author
            {
                ForeignAuthorId = "ce09ea31-3d4a-4487-a797-e315175457a0",
                Path = @"C:\Test\Music\Name1",
            };

            _fakeAuthor.Metadata = Builder<AuthorMetadata>.CreateNew().With(x => x.Disambiguation = "Disambiguation").Build();

            GivenValidAuthor(newAuthor.ForeignAuthorId);
            GivenValidPath();

            Mocker.GetMock<IAuthorService>()
                .Setup(x => x.AuthorPathExists(newAuthor.Path))
                .Returns(true);

            var author = Subject.AddAuthor(newAuthor);
            author.Path.Should().Be(newAuthor.Path + " (Disambiguation)");
        }

        [Test]
        public void should_disambiguate_with_numbers_if_author_folder_still_exists()
        {
            var newAuthor = new Author
            {
                ForeignAuthorId = "ce09ea31-3d4a-4487-a797-e315175457a0",
                Path = @"C:\Test\Music\Name1",
            };

            _fakeAuthor.Metadata = Builder<AuthorMetadata>.CreateNew().With(x => x.Disambiguation = "Disambiguation").Build();

            GivenValidAuthor(newAuthor.ForeignAuthorId);
            GivenValidPath();

            Mocker.GetMock<IAuthorService>()
                .Setup(x => x.AuthorPathExists(newAuthor.Path))
                .Returns(true);

            Mocker.GetMock<IAuthorService>()
                .Setup(x => x.AuthorPathExists(newAuthor.Path + " (Disambiguation)"))
                .Returns(true);

            Mocker.GetMock<IAuthorService>()
                .Setup(x => x.AuthorPathExists(newAuthor.Path + " (Disambiguation) (1)"))
                .Returns(true);

            Mocker.GetMock<IAuthorService>()
                .Setup(x => x.AuthorPathExists(newAuthor.Path + " (Disambiguation) (2)"))
                .Returns(true);

            var author = Subject.AddAuthor(newAuthor);
            author.Path.Should().Be(newAuthor.Path + " (Disambiguation) (3)");
        }

        [Test]
        public void should_disambiguate_with_numbers_if_author_folder_exists_and_no_disambiguation()
        {
            var newAuthor = new Author
            {
                ForeignAuthorId = "ce09ea31-3d4a-4487-a797-e315175457a0",
                Path = @"C:\Test\Music\Name1",
            };

            _fakeAuthor.Metadata = Builder<AuthorMetadata>.CreateNew().With(x => x.Disambiguation = string.Empty).Build();

            GivenValidAuthor(newAuthor.ForeignAuthorId);
            GivenValidPath();

            Mocker.GetMock<IAuthorService>()
                .Setup(x => x.AuthorPathExists(newAuthor.Path))
                .Returns(true);

            Mocker.GetMock<IAuthorService>()
                .Setup(x => x.AuthorPathExists(newAuthor.Path + " (1)"))
                .Returns(true);

            Mocker.GetMock<IAuthorService>()
                .Setup(x => x.AuthorPathExists(newAuthor.Path + " (2)"))
                .Returns(true);

            var author = Subject.AddAuthor(newAuthor);
            author.Path.Should().Be(newAuthor.Path + " (3)");
        }
    }
}
