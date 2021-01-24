using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.MetadataSource.Goodreads;
using NzbDrone.Core.Profiles.Metadata;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MetadataSource.Goodreads
{
    [TestFixture]
    public class GoodreadsProxySearchFixture : CoreTest<GoodreadsProxy>
    {
        [SetUp]
        public void Setup()
        {
            UseRealHttp();

            var metadataProfile = new MetadataProfile();

            Mocker.GetMock<IMetadataProfileService>()
                .Setup(s => s.All())
                .Returns(new List<MetadataProfile> { metadataProfile });

            Mocker.GetMock<IMetadataProfileService>()
                .Setup(s => s.Get(It.IsAny<int>()))
                .Returns(metadataProfile);
        }

        [TestCase("Robert Harris", "Robert Harris")]
        [TestCase("James Patterson", "James Patterson")]
        [TestCase("Antoine de Saint-Exupéry", "Antoine de Saint-Exupéry")]
        public void successful_author_search(string title, string expected)
        {
            var result = Subject.SearchForNewAuthor(title);

            result.Should().NotBeEmpty();

            result[0].Name.Should().Be(expected);

            ExceptionVerification.IgnoreWarns();
        }

        [TestCase("Harry Potter and the sorcerer's stone", null, "Harry Potter and the Sorcerer's Stone")]
        [TestCase("readarr:3", null, "Harry Potter and the Philosopher's Stone")]
        [TestCase("readarr: 3", null, "Harry Potter and the Philosopher's Stone")]
        [TestCase("readarrid:3", null, "Harry Potter and the Philosopher's Stone")]
        [TestCase("goodreads:3", null, "Harry Potter and the Philosopher's Stone")]
        [TestCase("asin:B0192CTMYG", null, "Harry Potter and the Sorcerer's Stone")]
        [TestCase("isbn:9780439554930", null, "Harry Potter and the Sorcerer's Stone")]
        public void successful_book_search(string title, string author, string expected)
        {
            var result = Subject.SearchForNewBook(title, author);

            result.Should().NotBeEmpty();

            result[0].Title.Should().Be(expected);

            ExceptionVerification.IgnoreWarns();
        }

        [TestCase("readarrid:")]
        [TestCase("readarrid: 99999999999999999999")]
        [TestCase("readarrid: 0")]
        [TestCase("readarrid: -12")]
        [TestCase("readarrid: aaaa")]
        [TestCase("adjalkwdjkalwdjklawjdlKAJD")]
        public void no_author_search_result(string term)
        {
            var result = Subject.SearchForNewAuthor(term);
            result.Should().BeEmpty();

            ExceptionVerification.IgnoreWarns();
        }

        [TestCase("Philip Pullman", 0, typeof(Author), "Philip Pullman")]
        [TestCase("Philip Pullman", 1, typeof(Book), "The Golden Compass")]
        public void successful_combined_search(string query, int position, Type resultType, string expected)
        {
            var result = Subject.SearchForNewEntity(query);
            result.Should().NotBeEmpty();
            result[position].GetType().Should().Be(resultType);

            if (resultType == typeof(Author))
            {
                var cast = result[position] as Author;
                cast.Should().NotBeNull();
                cast.Name.Should().Be(expected);
            }
            else
            {
                var cast = result[position] as Book;
                cast.Should().NotBeNull();
                cast.Title.Should().Be(expected);
            }
        }
    }
}
