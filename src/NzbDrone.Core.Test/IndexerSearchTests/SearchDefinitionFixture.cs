using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerSearchTests
{
    public class BookSearchDefinitionFixture : CoreTest<BookSearchCriteria>
    {
        [TestCase("Mötley Crüe", "Motley+Crue")]
        [TestCase("방탄소년단", "방탄소년단")]
        public void should_replace_some_special_characters_author(string author, string expected)
        {
            Subject.Author = new Author { Name = author };
            Subject.AuthorQuery.Should().Be(expected);
        }

        [TestCase("…and Justice for All", "and+Justice+for+All")]
        [TestCase("American III: Solitary Man", "American+III+Solitary+Man")]
        [TestCase("Sad Clowns & Hillbillies", "Sad+Clowns+Hillbillies")]
        [TestCase("¿Quién sabe?", "Quien+sabe")]
        [TestCase("Seal the Deal & Let’s Boogie", "Seal+the+Deal+Let’s+Boogie")]
        [TestCase("Section.80", "Section+80")]
        public void should_replace_some_special_characters(string book, string expected)
        {
            Subject.BookTitle = book;
            Subject.BookQuery.Should().Be(expected);
        }

        [TestCase("+", "+")]
        public void should_not_replace_some_special_characters_if_result_empty_string(string book, string expected)
        {
            Subject.BookTitle = book;
            Subject.BookQuery.Should().Be(expected);
        }
    }
}
