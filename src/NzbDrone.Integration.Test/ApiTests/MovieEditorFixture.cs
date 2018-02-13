using FluentAssertions;
using NUnit.Framework;
using System.Linq;
using NzbDrone.Test.Common;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class MovieEditorFixture : IntegrationTest
    {
        private void GivenExistingMovie()
        {
            foreach (var title in new[] { "90210", "Dexter" })
            {
                var newMovie = Movies.Lookup(title).First();

                newMovie.ProfileId = 1;
                newMovie.Path = string.Format(@"C:\Test\{0}", title).AsOsAgnostic();

                Movies.Post(newMovie);
            }
        }

        [Test]
        public void should_be_able_to_update_multiple_movies()
        {
            GivenExistingMovie();

            var movie = Movies.All();

            foreach (var s in movie)
            {
                s.ProfileId = 2;
            }

            var result = Movies.Editor(movie);

            result.Should().HaveCount(2);
            result.TrueForAll(s => s.ProfileId == 2).Should().BeTrue();
        }
    }
}
