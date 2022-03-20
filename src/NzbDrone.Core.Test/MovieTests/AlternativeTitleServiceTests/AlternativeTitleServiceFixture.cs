using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.AlternativeTitles;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MovieTests.AlternativeTitleServiceTests
{
    [TestFixture]
    public class AlternativeTitleServiceFixture : CoreTest<AlternativeTitleService>
    {
        private AlternativeTitle _title1;
        private AlternativeTitle _title2;
        private AlternativeTitle _title3;

        private MovieMetadata _movie;

        [SetUp]
        public void Setup()
        {
            var titles = Builder<AlternativeTitle>.CreateListOfSize(3).All().With(t => t.MovieMetadataId = 0).Build();
            _title1 = titles[0];
            _title2 = titles[1];
            _title3 = titles[2];

            _movie = Builder<MovieMetadata>.CreateNew()
                .With(m => m.CleanTitle = "myothertitle")
                .With(m => m.Id = 1)
                .Build();
        }

        private void GivenExistingTitles(params AlternativeTitle[] titles)
        {
            Mocker.GetMock<IAlternativeTitleRepository>().Setup(r => r.FindByMovieMetadataId(_movie.Id))
                .Returns(titles.ToList());
        }

        [Test]
        public void should_update_insert_remove_titles()
        {
            var titles = new List<AlternativeTitle> { _title2, _title3 };
            var updates = new List<AlternativeTitle> { _title2 };
            var deletes = new List<AlternativeTitle> { _title1 };
            var inserts = new List<AlternativeTitle> { _title3 };
            GivenExistingTitles(_title1, _title2);

            Subject.UpdateTitles(titles, _movie);

            Mocker.GetMock<IAlternativeTitleRepository>().Verify(r => r.InsertMany(inserts), Times.Once());
            Mocker.GetMock<IAlternativeTitleRepository>().Verify(r => r.UpdateMany(updates), Times.Once());
            Mocker.GetMock<IAlternativeTitleRepository>().Verify(r => r.DeleteMany(deletes), Times.Once());
        }

        [Test]
        public void should_not_insert_duplicates()
        {
            GivenExistingTitles();
            var titles = new List<AlternativeTitle> { _title1, _title1 };
            var inserts = new List<AlternativeTitle> { _title1 };

            Subject.UpdateTitles(titles, _movie);

            Mocker.GetMock<IAlternativeTitleRepository>().Verify(r => r.InsertMany(inserts), Times.Once());
        }

        [Test]
        public void should_not_insert_main_title()
        {
            GivenExistingTitles();
            var titles = new List<AlternativeTitle> { _title1 };
            var movie = Builder<MovieMetadata>.CreateNew().With(m => m.CleanTitle = _title1.CleanTitle).Build();

            Subject.UpdateTitles(titles, movie);

            Mocker.GetMock<IAlternativeTitleRepository>().Verify(r => r.InsertMany(new List<AlternativeTitle>()), Times.Once());
        }

        [Test]
        public void should_update_movie_id()
        {
            GivenExistingTitles();
            var titles = new List<AlternativeTitle> { _title1, _title2 };

            Subject.UpdateTitles(titles, _movie);

            _title1.MovieMetadataId.Should().Be(_movie.Id);
            _title2.MovieMetadataId.Should().Be(_movie.Id);
        }

        [Test]
        public void should_update_with_correct_id()
        {
            var existingTitle = Builder<AlternativeTitle>.CreateNew().With(t => t.Id = 2).Build();
            GivenExistingTitles(existingTitle);
            var updateTitle = existingTitle.JsonClone();
            updateTitle.Id = 0;

            Subject.UpdateTitles(new List<AlternativeTitle> { updateTitle }, _movie);

            Mocker.GetMock<IAlternativeTitleRepository>().Verify(r => r.UpdateMany(It.Is<IList<AlternativeTitle>>(list => list.First().Id == existingTitle.Id)), Times.Once());
        }
    }
}
