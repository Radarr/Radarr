using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Credits;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MovieTests.AlternativeTitleServiceTests
{
    [TestFixture]
    public class CreditServiceFixture : CoreTest<CreditService>
    {
        private Credit _credit1;
        private Credit _credit2;
        private Credit _credit3;

        private MovieMetadata _movie;

        [SetUp]
        public void Setup()
        {
            var credits = Builder<Credit>.CreateListOfSize(3)
                                         .All()
                                         .With(t => t.MovieMetadataId = 0).Build();

            _credit1 = credits[0];
            _credit2 = credits[1];
            _credit3 = credits[2];

            _movie = Builder<MovieMetadata>.CreateNew().With(m => m.Id = 1).Build();
        }

        private void GivenExistingCredits(params Credit[] credits)
        {
            Mocker.GetMock<ICreditRepository>().Setup(r => r.FindByMovieMetadataId(_movie.Id))
                .Returns(credits.ToList());
        }

        [Test]
        public void should_update_insert_remove_titles()
        {
            var titles = new List<Credit> { _credit2, _credit3 };
            var updates = new List<Credit> { _credit2 };
            var deletes = new List<Credit> { _credit1 };
            var inserts = new List<Credit> { _credit3 };

            GivenExistingCredits(_credit1, _credit2);

            Subject.UpdateCredits(titles, _movie);

            Mocker.GetMock<ICreditRepository>().Verify(r => r.InsertMany(inserts), Times.Once());
            Mocker.GetMock<ICreditRepository>().Verify(r => r.UpdateMany(updates), Times.Once());
            Mocker.GetMock<ICreditRepository>().Verify(r => r.DeleteMany(deletes), Times.Once());
        }

        [Test]
        public void should_not_insert_duplicates()
        {
            GivenExistingCredits();
            var credits = new List<Credit> { _credit1, _credit1 };
            var inserts = new List<Credit> { _credit1 };

            Subject.UpdateCredits(credits, _movie);

            Mocker.GetMock<ICreditRepository>().Verify(r => r.InsertMany(inserts), Times.Once());
        }

        [Test]
        public void should_update_movie_id()
        {
            GivenExistingCredits();
            var titles = new List<Credit> { _credit1, _credit2 };

            Subject.UpdateCredits(titles, _movie);

            _credit1.MovieMetadataId.Should().Be(_movie.Id);
            _credit2.MovieMetadataId.Should().Be(_movie.Id);
        }

        [Test]
        public void should_update_with_correct_id()
        {
            var existingCredit = Builder<Credit>.CreateNew().With(t => t.Id = 2).Build();

            GivenExistingCredits(existingCredit);

            var updateCredit = existingCredit.JsonClone();
            updateCredit.Id = 0;

            Subject.UpdateCredits(new List<Credit> { updateCredit }, _movie);

            Mocker.GetMock<ICreditRepository>().Verify(r => r.UpdateMany(It.Is<IList<Credit>>(list => list.First().Id == existingCredit.Id)), Times.Once());
        }
    }
}
