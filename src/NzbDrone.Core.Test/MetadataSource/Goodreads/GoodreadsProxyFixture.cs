using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MetadataSource.Goodreads;
using NzbDrone.Core.Profiles.Metadata;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MetadataSource.Goodreads
{
    [TestFixture]
    public class GoodreadsProxyFixture : CoreTest<GoodreadsProxy>
    {
        private MetadataProfile _metadataProfile;

        [SetUp]
        public void Setup()
        {
            UseRealHttp();

            _metadataProfile = new MetadataProfile();

            Mocker.GetMock<IMetadataProfileService>()
                .Setup(s => s.Get(It.IsAny<int>()))
                .Returns(_metadataProfile);

            Mocker.GetMock<IMetadataProfileService>()
                .Setup(s => s.Exists(It.IsAny<int>()))
                .Returns(true);
        }

        [TestCase("1654", "Terry Pratchett")]
        [TestCase("575", "Robert Harris")]
        public void should_be_able_to_get_author_detail(string mbId, string name)
        {
            var details = Subject.GetAuthorInfo(mbId);

            ValidateAuthor(details);

            details.Name.Should().Be(name);
        }

        [TestCase("64216", "Guards! Guards!")]
        [TestCase("1371", "Ιλιάς")]
        public void should_be_able_to_get_book_detail(string mbId, string name)
        {
            var details = Subject.GetBookInfo(mbId);

            ValidateBooks(new List<Book> { details.Item2 });

            details.Item2.Title.Should().Be(name);
        }

        [Test]
        public void getting_details_of_invalid_author()
        {
            Assert.Throws<AuthorNotFoundException>(() => Subject.GetAuthorInfo("66c66aaa-6e2f-4930-8610-912e24c63ed1"));
        }

        [Test]
        public void getting_details_of_invalid_book()
        {
            Assert.Throws<BookNotFoundException>(() => Subject.GetBookInfo("66c66aaa-6e2f-4930-8610-912e24c63ed1"));
        }

        private void ValidateAuthor(Author author)
        {
            author.Should().NotBeNull();
            author.Name.Should().NotBeNullOrWhiteSpace();
            author.CleanName.Should().Be(Parser.Parser.CleanAuthorName(author.Name));
            author.SortName.Should().Be(Parser.Parser.NormalizeTitle(author.Name));
            author.Metadata.Value.TitleSlug.Should().NotBeNullOrWhiteSpace();
            author.Metadata.Value.Overview.Should().NotBeNullOrWhiteSpace();
            author.Metadata.Value.Images.Should().NotBeEmpty();
            author.ForeignAuthorId.Should().NotBeNullOrWhiteSpace();
        }

        private void ValidateBooks(List<Book> books, bool idOnly = false)
        {
            books.Should().NotBeEmpty();

            foreach (var book in books)
            {
                book.ForeignBookId.Should().NotBeNullOrWhiteSpace();
                if (!idOnly)
                {
                    ValidateBook(book);
                }
            }

            //if atleast one book has title it means parse it working.
            if (!idOnly)
            {
                books.Should().Contain(c => !string.IsNullOrWhiteSpace(c.Title));
            }
        }

        private void ValidateBook(Book book)
        {
            book.Should().NotBeNull();

            book.Title.Should().NotBeNullOrWhiteSpace();

            book.Should().NotBeNull();

            if (book.ReleaseDate.HasValue)
            {
                book.ReleaseDate.Value.Kind.Should().Be(DateTimeKind.Utc);
            }
        }
    }
}
