using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class MovieFixture : IntegrationTest
    {
        [Test]
        [Order(0)]
        public void add_movie_with_tags_should_store_them()
        {
            EnsureNoMovie(680, "Pulp Fiction");
            var tag = EnsureTag("abc");

            var movie = Movies.Lookup("imdb:tt0110912").Single();

            movie.QualityProfileId = 1;
            movie.Path = Path.Combine(MovieRootFolder, movie.Title);
            movie.Tags = new HashSet<int>();
            movie.Tags.Add(tag.Id);

            var result = Movies.Post(movie);

            result.Should().NotBeNull();
            result.Tags.Should().Equal(tag.Id);
        }

        [Test]
        [Order(0)]
        public void add_movie_without_profileid_should_return_badrequest()
        {
            EnsureNoMovie(680, "Pulp Fiction");

            var movie = Movies.Lookup("imdb:tt0110912").Single();

            movie.Path = Path.Combine(MovieRootFolder, movie.Title);

            Movies.InvalidPost(movie);
        }

        [Test]
        [Order(0)]
        public void add_movie_without_path_should_return_badrequest()
        {
            EnsureNoMovie(680, "Pulp Fiction");

            var movie = Movies.Lookup("imdb:tt0110912").Single();

            movie.QualityProfileId = 1;

            Movies.InvalidPost(movie);
        }

        [Test]
        [Order(1)]
        public void add_movie()
        {
            EnsureNoMovie(680, "Pulp Fiction");

            var movie = Movies.Lookup("imdb:tt0110912").Single();

            movie.QualityProfileId = 1;
            movie.Path = Path.Combine(MovieRootFolder, movie.Title);

            var result = Movies.Post(movie);

            result.Should().NotBeNull();
            result.Id.Should().NotBe(0);
            result.QualityProfileId.Should().Be(1);
            result.Path.Should().Be(Path.Combine(MovieRootFolder, movie.Title));
        }

        [Test]
        [Order(2)]
        public void get_all_movies()
        {
            EnsureMovie(680, "Pulp Fiction");
            EnsureMovie(155, "The Dark Knight");

            var movies = Movies.All();

            movies.Should().NotBeNullOrEmpty();
            movies.Should().Contain(v => v.ImdbId == "tt0110912");
            movies.Should().Contain(v => v.ImdbId == "tt0468569");
            movies.Should().Contain(v => v.Images.All(i => i.RemoteUrl.Contains("https://image.tmdb.org")));
        }

        [Test]
        [Order(2)]
        public void get_movie_by_tmdbid()
        {
            EnsureMovie(680, "Pulp Fiction");
            EnsureMovie(155, "The Dark Knight");

            var queryParams = new Dictionary<string, object>()
            {
                { "tmdbId", 680 }
            };

            var movies = Movies.All(queryParams);

            movies.Should().NotBeNullOrEmpty();
            movies.Should().Contain(v => v.ImdbId == "tt0110912");
            movies.Should().Contain(v => v.Images.All(i => i.RemoteUrl.Contains("https://image.tmdb.org")));
        }

        [Test]
        [Order(2)]
        public void get_movie_by_id()
        {
            var movie = EnsureMovie(680, "Pulp Fiction");

            var result = Movies.Get(movie.Id);

            result.ImdbId.Should().Be("tt0110912");
        }

        [Test]
        public void get_movie_by_unknown_id_should_return_404()
        {
            var result = Movies.InvalidGet(1000000);
        }

        [Test]
        [Order(2)]
        public void update_movie_profile_id()
        {
            var movie = EnsureMovie(680, "Pulp Fiction");

            var profileId = 1;
            if (movie.QualityProfileId == profileId)
            {
                profileId = 2;
            }

            movie.QualityProfileId = profileId;

            var result = Movies.Put(movie);

            Movies.Get(movie.Id).QualityProfileId.Should().Be(profileId);
        }

        [Test]
        [Order(3)]
        public void update_movie_monitored()
        {
            var movie = EnsureMovie(680, "Pulp Fiction", false);

            movie.Monitored.Should().BeFalse();

            movie.Monitored = true;

            var result = Movies.Put(movie);

            result.Monitored.Should().BeTrue();
        }

        [Test]
        [Order(3)]
        public void update_movie_tags()
        {
            var movie = EnsureMovie(680, "Pulp Fiction");
            var tag = EnsureTag("abc");

            if (movie.Tags.Contains(tag.Id))
            {
                movie.Tags.Remove(tag.Id);

                var result = Movies.Put(movie);
                Movies.Get(movie.Id).Tags.Should().NotContain(tag.Id);
            }
            else
            {
                movie.Tags.Add(tag.Id);

                var result = Movies.Put(movie);
                Movies.Get(movie.Id).Tags.Should().Contain(tag.Id);
            }
        }

        [Test]
        [Order(4)]
        public void delete_movie()
        {
            var movie = EnsureMovie(680, "Pulp Fiction");

            Movies.Get(movie.Id).Should().NotBeNull();

            Movies.Delete(movie.Id);

            Movies.All().Should().NotContain(v => v.ImdbId == "tt0110912");
        }
    }
}
