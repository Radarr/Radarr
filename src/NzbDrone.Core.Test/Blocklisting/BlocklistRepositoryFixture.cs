using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Blocklisting;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Blocklisting
{
    [TestFixture]
    public class BlocklistRepositoryFixture : DbTest<BlocklistRepository, Blocklist>
    {
        private Blocklist _blocklist;
        private Movie _movie1;
        private Movie _movie2;

        [SetUp]
        public void Setup()
        {
            _blocklist = new Blocklist
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
            Subject.Insert(_blocklist);
            Subject.All().Should().HaveCount(1);
        }

        [Test]
        public void should_should_have_movie_id()
        {
            Subject.Insert(_blocklist);

            Subject.All().First().MovieId.Should().Be(_blocklist.MovieId);
        }

        [Test]
        public void should_check_for_blocklisted_title_case_insensative()
        {
            Subject.Insert(_blocklist);

            Subject.BlocklistedByTitle(_blocklist.MovieId, _blocklist.SourceTitle.ToUpperInvariant()).Should().HaveCount(1);
        }

        [Test]
        public void should_delete_blocklists_by_movieId()
        {
            var blocklistItems = Builder<Blocklist>.CreateListOfSize(5)
                .TheFirst(1)
                .With(c => c.MovieId = _movie2.Id)
                .TheRest()
                .With(c => c.MovieId = _movie1.Id)
                .All()
                .With(c => c.Quality = new QualityModel())
                .With(c => c.Languages = new List<Language>())
                .With(c => c.Id = 0)
                .BuildListOfNew();

            Db.InsertMany(blocklistItems);

            Subject.DeleteForMovies(new List<int> { _movie1.Id });

            var removedMovieBlocklists = Subject.BlocklistedByMovie(_movie1.Id);
            var nonRemovedMovieBlocklists = Subject.BlocklistedByMovie(_movie2.Id);

            removedMovieBlocklists.Should().HaveCount(0);
            nonRemovedMovieBlocklists.Should().HaveCount(1);
        }
    }
}
