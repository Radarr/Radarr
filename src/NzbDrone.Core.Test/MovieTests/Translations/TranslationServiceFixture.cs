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
using NzbDrone.Core.Parser;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MovieTests.TranslationServiceTests
{
    [TestFixture]
    public class TranslationServiceFixture : CoreTest<MovieTranslationService>
    {
        private MovieTranslation _translation1;
        private MovieTranslation _translation2;
        private MovieTranslation _translation3;

        // https://api.radarr.video/v1/movie/330459
        private static (string lang, string translation)[] _translations = new (string, string)[]
        {
            ("en-US", "Rogue One: A Star Wars Story"),
            ("es-ES", "Rogue One: Una historia de Star Wars"),
            ("ro-RO", "Rogue One: O poveste Star Wars"),
            ("fr-FR", "Rogue One : A Star Wars Story"),
            ("cs-CZ", "Rogue One: Star Wars Story"),
            ("el-GR", "Rogue One: A Star Wars Story"),
            ("ru-RU", "Изгой-один: Звёздные войны. Истории"),
            ("de-DE", "Rogue One: A Star Wars Story"),
            ("he-IL", "רוג אחת: סיפור מלחמת הכוכבים"),
            ("it-IT", "Rogue One: A Star Wars Story"),
            ("hu-HU", "Zsivány Egyes: Egy Star Wars-történet"),
            ("pt-BR", "Rogue One: Uma História Star Wars"),
            ("pl-PL", "Łotr 1. Gwiezdne wojny – historie"),
            ("nl-NL", "Rogue One: A Star Wars Story"),
            ("da-DK", "Rogue One: A Star Wars Story"),
            ("pt-PT", "Rogue One: Uma História de Star Wars"),
            ("ko-KR", "로그 원: 스타워즈 스토리"),
            ("uk-UA", "Бунтар Один. Зоряні війни: Історія"),
            ("tr-TR", "Rogue One: Bir Star Wars Hikayesi"),
            ("bs-BS", "Rogue One: A Star Wars Story"),
            ("ja-JP", "ローグ・ワン／スター・ウォーズ・ストーリー"),
            ("zh-CN", "星球大战外传：侠盗一号"),
            ("ar-SA", "روج وان: قصة من حرب النجوم"),
            ("hr-HR", "Rogue One: Priča iz Ratova zvijezda"),
            ("sv-SE", "Rogue One: A Star Wars Story"),
            ("bg-BG", "Rogue One: История от Междузвездни войни"),
            ("sk-SK", "Rogue One: A Star Wars Story"),
            ("es-MX", "Star Wars: Rogue One"),
            ("sr-RS", "ОДМЕТНИК-1: прича Ратова звезда"),
            ("sl-SI", "Rogue One: Zgodba vojne zvezd"),
            ("fi-FI", "Rogue One: A Star Wars Story"),
            ("th-TH", "โร้ค วัน ตำนานสตาร์ วอร์ส"),
            ("ml-IN", "റോഗ് വൺ: എ സ്റ്റാർ വാഴ്‌സ് സ്റ്റോറി"),
            ("vi-VN", "Rogue One: Star Wars Ngoại Truyện"),
            ("fa-IR", "یاغی: داستانی از جنگ ستارگان"),
            ("no-NO", "Rogue One - A Star Wars Story"),
            ("lv-LV", "Rogue One: Zvaigžņu karu stāsts"),
            ("lt-LT", "Šelmis-1. Žvaigždžių karų istorija"),
            ("et-EE", "Rogue One: Tähesõdade lugu")
        };
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
        public void should_remove_existing_duplicates()
        {
            var duplicated = _translation1.JsonClone();
            duplicated.Id = 2;
            GivenExistingTranslations(_translation1, duplicated);
            var translations = new List<MovieTranslation> { _translation1 };
            var deleted = new List<MovieTranslation> { duplicated };

            Subject.UpdateTranslations(translations, _movie);

            Mocker.GetMock<IMovieTranslationRepository>().Verify(r => r.DeleteMany(deleted), Times.Once());
        }

        [Test]
        public void should_not_update_existing_translations()
        {
            var translations = Builder<MovieTranslation>.CreateListOfSize(_translations.Length)
                .All()
                .With(t => t.MovieMetadataId = 0)
                .With((t, idx) => t.Title = _translations[idx].translation)
                .With((t, idx) => t.Language = IsoLanguages.Find(_translations[idx].lang).Language)
                .Build()
                .ToList();
            GivenExistingTranslations(translations.ToArray());

            Subject.UpdateTranslations(translations, _movie);

            Mocker.GetMock<IMovieTranslationRepository>().Verify(r => r.InsertMany(new List<MovieTranslation>()), Times.Once());
            Mocker.GetMock<IMovieTranslationRepository>().Verify(r => r.UpdateMany(new List<MovieTranslation>()), Times.Once());
            Mocker.GetMock<IMovieTranslationRepository>().Verify(r => r.DeleteMany(new List<MovieTranslation>()), Times.Once());
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
