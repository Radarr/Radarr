using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MusicTests.AuthorServiceTests
{
    [TestFixture]

    public class FindByNameInexactFixture : CoreTest<AuthorService>
    {
        private List<Author> _authors;

        private Author CreateAuthor(string name)
        {
            return Builder<Author>.CreateNew()
                .With(a => a.Name = name)
                .With(a => a.CleanName = Parser.Parser.CleanAuthorName(name))
                .With(a => a.ForeignAuthorId = name)
                .BuildNew();
        }

        [SetUp]
        public void Setup()
        {
            _authors = new List<Author>();
            _authors.Add(CreateAuthor("The Black Eyed Peas"));
            _authors.Add(CreateAuthor("The Black Keys"));

            Mocker.GetMock<IAuthorRepository>()
                .Setup(s => s.All())
                .Returns(_authors);
        }

        [TestCase("The Black Eyde Peas", "The Black Eyed Peas")]
        [TestCase("Black Eyed Peas", "The Black Eyed Peas")]
        [TestCase("The Black eys", "The Black Keys")]
        [TestCase("Black Keys", "The Black Keys")]
        public void should_find_author_in_db_by_name_inexact(string name, string expected)
        {
            var author = Subject.FindByNameInexact(name);

            author.Should().NotBeNull();
            author.Name.Should().Be(expected);
        }

        [Test]
        public void should_find_author_when_the_is_omitted_from_start()
        {
            _authors = new List<Author>();
            _authors.Add(CreateAuthor("Black Keys"));
            _authors.Add(CreateAuthor("The Black Eyed Peas"));

            Mocker.GetMock<IAuthorRepository>()
                .Setup(s => s.All())
                .Returns(_authors);

            Subject.FindByNameInexact("The Black Keys").Should().NotBeNull();
        }

        [TestCase("The Black Peas")]
        public void should_not_find_author_in_db_by_ambiguous_name(string name)
        {
            var author = Subject.FindByNameInexact(name);

            author.Should().BeNull();
        }
    }
}
