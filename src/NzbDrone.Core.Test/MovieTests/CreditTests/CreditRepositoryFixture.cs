using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Credits;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MovieTests.CreditTests
{
    [TestFixture]
    public class CreditRepositoryFixture : DbTest<CreditRepository, Credit>
    {
        private Movie _movie1;
        private Movie _movie2;

        [SetUp]
        public void Setup()
        {
            _movie1 = Builder<Movie>.CreateNew()
                                     .With(s => s.Id = 7)
                                     .Build();

            _movie2 = Builder<Movie>.CreateNew()
                                     .With(s => s.Id = 8)
                                     .Build();
        }

        [Test]
        public void should_delete_credits_by_movieId()
        {
            var credits = Builder<Credit>.CreateListOfSize(5)
                .TheFirst(1)
                .With(c => c.Id = 0)
                .With(c => c.MovieId = _movie2.Id)
                .TheRest()
                .With(c => c.Id = 0)
                .With(c => c.MovieId = _movie1.Id)
                .BuildListOfNew();

            Db.InsertMany(credits);

            Subject.DeleteForMovies(new List<int> { _movie1.Id });

            var removedMovieCredits = Subject.FindByMovieId(_movie1.Id);
            var nonRemovedMovieCredits = Subject.FindByMovieId(_movie2.Id);

            removedMovieCredits.Should().HaveCount(0);
            nonRemovedMovieCredits.Should().HaveCount(1);
        }
    }
}
