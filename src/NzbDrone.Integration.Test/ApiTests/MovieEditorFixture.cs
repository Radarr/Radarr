using FluentAssertions;
using NUnit.Framework;
using System.Linq;
using Radarr.Api.V2.Movies;
using NzbDrone.Test.Common;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class MovieEditorFixture : IntegrationTest
    {
        private void GivenExistingMovie()
        {
            foreach (var title in new[] { "The Dark Knight", "Pulp Fiction" })
            {
                var newMovie = Movies.Lookup(title).First();

                newMovie.QualityProfileId = 1;
                newMovie.Path = string.Format(@"C:\Test\{0}", title).AsOsAgnostic();

                Movies.Post(newMovie);
            }
        }

        [Test]
        public void should_be_able_to_update_multiple_movies()
        {
            GivenExistingMovie();

            var movies = Movies.All();

            var movieEditor = new MovieEditorResource
            {
                QualityProfileId = 2,
                MovieIds = movies.Select(o => o.Id).ToList()
            };

            var result = Movies.Editor(movieEditor);

            result.Should().HaveCount(2);
            result.TrueForAll(s => s.QualityProfileId == 2).Should().BeTrue();
        }
    }
}
