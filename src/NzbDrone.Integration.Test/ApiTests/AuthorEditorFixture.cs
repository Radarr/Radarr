using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Test.Common;
using Readarr.Api.V1.Author;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class AuthorEditorFixture : IntegrationTest
    {
        private void GivenExistingAuthor()
        {
            foreach (var name in new[] { "Alien Ant Farm", "Kiss" })
            {
                var newAuthor = Author.Lookup(name).First();

                newAuthor.QualityProfileId = 1;
                newAuthor.MetadataProfileId = 1;
                newAuthor.Path = string.Format(@"C:\Test\{0}", name).AsOsAgnostic();

                Author.Post(newAuthor);
            }
        }

        [Test]
        public void should_be_able_to_update_multiple_author()
        {
            GivenExistingAuthor();

            var author = Author.All();

            var authorEditor = new AuthorEditorResource
            {
                QualityProfileId = 2,
                AuthorIds = author.Select(o => o.Id).ToList()
            };

            var result = Author.Editor(authorEditor);

            result.Should().HaveCount(2);
            result.TrueForAll(s => s.QualityProfileId == 2).Should().BeTrue();
        }
    }
}
