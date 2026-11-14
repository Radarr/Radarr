using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Movies.Versions;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MovieTests.MovieVersionTests
{
    [TestFixture]
    public class MovieVersionRepositoryFixture : DbTest<MovieVersionRepository, MovieVersion>
    {
        private MovieVersion Version(int movieId, string path)
        {
            return Builder<MovieVersion>.CreateNew()
                                        .With(v => v.Id = 0)
                                        .With(v => v.MovieId = movieId)
                                        .With(v => v.Path = path)
                                        .BuildNew();
        }

        [Test]
        public void should_find_all_versions_of_a_movie()
        {
            Subject.Insert(Version(1, @"/Movies/Movie (2020)".AsOsAgnostic()));
            Subject.Insert(Version(1, @"/Movies 4K/Movie (2020)".AsOsAgnostic()));
            Subject.Insert(Version(2, @"/Movies/Other (2021)".AsOsAgnostic()));

            Subject.FindByMovieId(1).Should().HaveCount(2);
            Subject.FindByMovieIds(new List<int> { 1, 2 }).Should().HaveCount(3);
        }

        [Test]
        public void should_find_version_by_path()
        {
            Subject.Insert(Version(1, @"/Movies/Movie (2020)".AsOsAgnostic()));
            Subject.Insert(Version(1, @"/Movies 4K/Movie (2020)".AsOsAgnostic()));

            var version = Subject.FindByPath(@"/Movies 4K/Movie (2020)".AsOsAgnostic());

            version.Should().NotBeNull();
            version.Path.Should().Be(@"/Movies 4K/Movie (2020)".AsOsAgnostic());
        }

        [Test]
        public void should_return_null_when_no_version_matches_path()
        {
            Subject.Insert(Version(1, @"/Movies/Movie (2020)".AsOsAgnostic()));

            Subject.FindByPath(@"/Elsewhere/Movie (2020)".AsOsAgnostic()).Should().BeNull();
        }

        [Test]
        public void should_delete_versions_of_given_movies_only()
        {
            Subject.Insert(Version(1, @"/Movies/Movie (2020)".AsOsAgnostic()));
            Subject.Insert(Version(1, @"/Movies 4K/Movie (2020)".AsOsAgnostic()));
            Subject.Insert(Version(2, @"/Movies/Other (2021)".AsOsAgnostic()));

            Subject.DeleteForMovies(new List<int> { 1 });

            AllStoredModels.Should().ContainSingle(v => v.MovieId == 2);
        }
    }
}
