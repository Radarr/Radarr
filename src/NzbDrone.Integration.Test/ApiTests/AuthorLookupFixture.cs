using FluentAssertions;
using NUnit.Framework;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class AuthorLookupFixture : IntegrationTest
    {
        [TestCase("Robert Harris", "Robert Harris")]
        [TestCase("J.K. Rowling", "J.K. Rowling")]
        public void lookup_new_author_by_name(string term, string name)
        {
            var author = Author.Lookup(term);

            author.Should().NotBeEmpty();
            author.Should().Contain(c => c.AuthorName == name);
        }

        [Test]
        public void lookup_new_author_by_goodreads_book_id()
        {
            var author = Author.Lookup("readarr:1");

            author.Should().NotBeEmpty();
            author.Should().Contain(c => c.AuthorName == "J.K. Rowling");
        }
    }
}
