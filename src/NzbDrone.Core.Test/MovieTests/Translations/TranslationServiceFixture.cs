using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Translations;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MovieTests.TranslationServiceTests
{
    [TestFixture]
    public class TranslationServiceFixture : CoreTest<MovieTranslationService>
    {
        private MovieTranslation _translation1;
        private MovieTranslation _translation2;
        private MovieTranslation _translation3;

        private MovieMetadata _movie;

        [SetUp]
        public void Setup()
        {
            var generated = 3;
            var langs = Pick<Language>.UniqueRandomList(generated).From(Language.All);
            var translations = Builder<MovieTranslation>.CreateListOfSize(generated).All()
                .With(t => t.MovieMetadataId = 0)
                .With((t, idx) => t.Language = langs[idx]).Build();
            _translation1 = translations[0];
            _translation2 = translations[1];
            _translation3 = translations[2];

            _movie = Builder<MovieMetadata>.CreateNew()
                .With(m => m.CleanTitle = "myothertitle")
                .With(m => m.Id = 1)
                .Build();
        }

        private void GivenExistingTranslations(params MovieTranslation[] titles)
        {
            Mocker.GetMock<IMovieTranslationRepository>().Setup(r => r.FindByMovieMetadataId(_movie.Id))
                .Returns(titles.ToList());
        }

        [Test]
        public void should_update_insert_remove_titles()
        {
            // Deep copy of _title2, but change Title
            // to ensure it gets into the update list
            var updatedTranslation2 = _translation2.JsonClone();
            updatedTranslation2.Title = updatedTranslation2.Title + "TEST";
            var translations = new List<MovieTranslation> { updatedTranslation2, _translation3 };
            var updates = new List<MovieTranslation> { updatedTranslation2 };
            var deletes = new List<MovieTranslation> { _translation1 };
            var inserts = new List<MovieTranslation> { _translation3 };
            GivenExistingTranslations(_translation1, _translation2);

            Subject.UpdateTranslations(translations, _movie);

            Mocker.GetMock<IMovieTranslationRepository>().Verify(r => r.InsertMany(inserts), Times.Once());
            Mocker.GetMock<IMovieTranslationRepository>().Verify(r => r.UpdateMany(updates), Times.Once());
            Mocker.GetMock<IMovieTranslationRepository>().Verify(r => r.DeleteMany(deletes), Times.Once());
        }

        [Test]
        public void should_not_insert_duplicates()
        {
            GivenExistingTranslations();
            var translations = new List<MovieTranslation> { _translation1, _translation1 };
            var inserts = new List<MovieTranslation> { _translation1 };

            Subject.UpdateTranslations(translations, _movie);

            Mocker.GetMock<IMovieTranslationRepository>().Verify(r => r.InsertMany(inserts), Times.Once());
        }

        [Test]
        public void should_update_movie_id()
        {
            GivenExistingTranslations();
            var translations = new List<MovieTranslation> { _translation1, _translation2 };

            Subject.UpdateTranslations(translations, _movie);

            _translation1.MovieMetadataId.Should().Be(_movie.Id);
            _translation2.MovieMetadataId.Should().Be(_movie.Id);
        }

        [Test]
        public void should_update_with_correct_id()
        {
            var existingTranslations = Builder<MovieTranslation>.CreateNew()
                .With(t => t.Id = 2)
                .With(t => t.Language = Language.French).Build();
            GivenExistingTranslations(existingTranslations);
            var updateTranslations = existingTranslations.JsonClone();
            updateTranslations.Overview = "Overview";
            updateTranslations.Id = 0;

            Subject.UpdateTranslations(new List<MovieTranslation> { updateTranslations }, _movie);

            Mocker.GetMock<IMovieTranslationRepository>().Verify(r => r.UpdateMany(It.Is<IList<MovieTranslation>>(list => list.First().Id == existingTranslations.Id)), Times.Once());
        }

        [Test]
        public void should_not_update_same_translations()
        {
            GivenExistingTranslations(_translation1);

            Subject.UpdateTranslations(new List<MovieTranslation> { _translation1 }, _movie);

            Mocker.GetMock<IMovieTranslationRepository>().Verify(r => r.UpdateMany(new List<MovieTranslation>()), Times.Once());
        }
    }
}
