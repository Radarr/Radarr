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

        // https://api.radarr.video/v1/movie/330459
        private static string[] _titles = new string[]
        {
            "Star Wars: Rogue One",
            "Rogue One: Uma História de Guerra nas Estrelas",
            "Star Wars, Episode 3.5 - Rogue One",
            "Star Wars, épisode III bis - Rogue One",
            "로그 원: 스타 워즈 스토리",
            "Star Wars: Rogue One",
            "로그원: 스타워즈 스토리",
            "스타워즈-로그원",
            "Star Wars: Episode III.V - Rogue One: A Star Wars Story",
            "Rogue One: Hvězdné války",
            "Star Wars - Rogue One: Una historia de Star Wars",
            "Rogue One: A Star Wars Story",
            "Zsivány Egyes: Egy Star Wars-történet",
            "Star Wars, épisode III.2 - Rogue One",
            "Haydut: Bir Yıldız Savaşları Hikayesi",
            "Rogue One: Chiến Tranh Giữa Các Vì Sao Ngoại Truyện",
            "Күрескер бір: Жұлдызды соғыстар хикаясы",
            "Rogue One: Tähesõdade lugu",
            "Rogue One",
            "La guerra de las galaxias. Rogue One: Una historia de Star Wars",
            "俠盜一號：星球大戰外傳",
            "Rogue One - A Star Wars Story"
        };

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
            // Deep copy of _title2, but change Title
            // to ensure it gets into the update list
            var updatedTitle2 = _title2.JsonClone();
            updatedTitle2.Title = updatedTitle2.Title + "TEST";
            var titles = new List<AlternativeTitle> { updatedTitle2, _title3 };
            var updates = new List<AlternativeTitle> { updatedTitle2 };
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
        public void should_not_update_existing_titles()
        {
            var titles = Builder<AlternativeTitle>.CreateListOfSize(_titles.Length)
                .All()
                .With(t => t.MovieMetadataId = 0)
                .With((t, idx) => t.Title = _titles[idx])
                .Build()
                .ToList();
            GivenExistingTitles(titles.ToArray());

            Subject.UpdateTitles(titles, _movie);

            Mocker.GetMock<IAlternativeTitleRepository>().Verify(r => r.InsertMany(new List<AlternativeTitle>()), Times.Once());
            Mocker.GetMock<IAlternativeTitleRepository>().Verify(r => r.UpdateMany(new List<AlternativeTitle>()), Times.Once());
            Mocker.GetMock<IAlternativeTitleRepository>().Verify(r => r.DeleteMany(new List<AlternativeTitle>()), Times.Once());
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
            updateTitle.Title = updateTitle.Title + "TEST";
            updateTitle.Id = 0;

            Subject.UpdateTitles(new List<AlternativeTitle> { updateTitle }, _movie);

            Mocker.GetMock<IAlternativeTitleRepository>().Verify(r => r.UpdateMany(It.Is<IList<AlternativeTitle>>(list => list.First().Id == existingTitle.Id)), Times.Once());
        }

        [Test]
        public void should_remove_existing_duplicates()
        {
            var duplicated = _title1.JsonClone();
            duplicated.Id = 2;
            GivenExistingTitles(_title1, duplicated);
            var translations = new List<AlternativeTitle> { _title1 };
            var deleted = new List<AlternativeTitle> { duplicated };

            Subject.UpdateTitles(translations, _movie);

            Mocker.GetMock<IAlternativeTitleRepository>().Verify(r => r.DeleteMany(deleted), Times.Once());
        }

        [Test]
        public void should_not_update_same_translations()
        {
            GivenExistingTitles(_title1);

            Subject.UpdateTitles(new List<AlternativeTitle> { _title1 }, _movie);

            Mocker.GetMock<IAlternativeTitleRepository>().Verify(r => r.UpdateMany(new List<AlternativeTitle>()), Times.Once());
        }
    }
}
