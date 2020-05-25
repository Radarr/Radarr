using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Blacklisting;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Blacklisting
{
    [TestFixture]
    public class BlacklistRepositoryFixture : DbTest<BlacklistRepository, Blacklist>
    {
        private Blacklist _blacklist;
        private Movie _movie1;
        private Movie _movie2;

        [SetUp]
        public void Setup()
        {
            _blacklist = new Blacklist
            {
                MovieId = 1234,
                Quality = new QualityModel(),
                Languages = new List<Language>(),
                SourceTitle = "movie.title.1998",
                Date = DateTime.UtcNow
            };

            _movie1 = Builder<Movie>.CreateNew()
                         .With(s => s.Id = 7)
                         .Build();

            _movie2 = Builder<Movie>.CreateNew()
                                     .With(s => s.Id = 8)
                                     .Build();
        }

        [Test]
        public void should_be_able_to_write_to_database()
        {
            Subject.Insert(_blacklist);
            Subject.All().Should().HaveCount(1);
        }

        [Test]
        public void should_should_have_movie_id()
        {
            Subject.Insert(_blacklist);

            Subject.All().First().MovieId.Should().Be(_blacklist.MovieId);
        }

        [Test]
        public void should_check_for_blacklisted_title_case_insensative()
        {
            Subject.Insert(_blacklist);

            Subject.BlacklistedByTitle(_blacklist.MovieId, _blacklist.SourceTitle.ToUpperInvariant()).Should().HaveCount(1);
        }

        [Test]
        public void should_delete_blacklists_by_movieId()
        {
            var blacklistItems = Builder<Blacklist>.CreateListOfSize(5)
                .TheFirst(1)
                .With(c => c.MovieId = _movie2.Id)
                .TheRest()
                .With(c => c.MovieId = _movie1.Id)
                .All()
                .With(c => c.Quality = new QualityModel())
                .With(c => c.Languages = new List<Language>())
                .With(c => c.Id = 0)
                .BuildListOfNew();

            Db.InsertMany(blacklistItems);

            Subject.DeleteForMovies(new List<int> { _movie1.Id });

            var removedMovieBlacklists = Subject.BlacklistedByMovies(new List<int> { _movie1.Id });
            var nonRemovedMovieBlacklists = Subject.BlacklistedByMovies(new List<int> { _movie2.Id });

            removedMovieBlacklists.Should().HaveCount(0);
            nonRemovedMovieBlacklists.Should().HaveCount(1);
        }
    }
}
