using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Movies;
using NzbDrone.Core.NetImport;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Test.CustomFormats;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Profiles
{
    [TestFixture]

    public class ProfileServiceFixture : CoreTest<ProfileService>
    {
        [Test]
        public void init_should_add_default_profiles()
        {
            Mocker.GetMock<ICustomFormatService>()
                .Setup(s => s.All())
                .Returns(new List<CustomFormat>());

            Subject.Handle(new ApplicationStartedEvent());

            Mocker.GetMock<IProfileRepository>()
                .Verify(v => v.Insert(It.IsAny<Profile>()), Times.Exactly(6));
        }

        [Test]

        //This confirms that new profiles are added only if no other profiles exists.
        //We don't want to keep adding them back if a user deleted them on purpose.
        public void Init_should_skip_if_any_profiles_already_exist()
        {
            Mocker.GetMock<IProfileRepository>()
                  .Setup(s => s.All())
                  .Returns(Builder<Profile>.CreateListOfSize(2).Build().ToList());

            Subject.Handle(new ApplicationStartedEvent());

            Mocker.GetMock<IProfileRepository>()
                .Verify(v => v.Insert(It.IsAny<Profile>()), Times.Never());
        }

        [Test]
        public void should_not_be_able_to_delete_profile_if_assigned_to_movie()
        {
            var movieList = Builder<Movie>.CreateListOfSize(3)
                                            .Random(1)
                                            .With(c => c.ProfileId = 2)
                                            .Build().ToList();

            var netImportList = Builder<NetImportDefinition>.CreateListOfSize(3)
                                                            .All()
                                                            .With(c => c.ProfileId = 1)
                                                            .Build().ToList();

            Mocker.GetMock<IMovieService>().Setup(c => c.GetAllMovies()).Returns(movieList);
            Mocker.GetMock<INetImportFactory>().Setup(c => c.All()).Returns(netImportList);

            Assert.Throws<ProfileInUseException>(() => Subject.Delete(2));

            Mocker.GetMock<IProfileRepository>().Verify(c => c.Delete(It.IsAny<int>()), Times.Never());
        }

        [Test]
        public void should_not_be_able_to_delete_profile_if_assigned_to_list()
        {
            var movieList = Builder<Movie>.CreateListOfSize(3)
                .All()
                .With(c => c.ProfileId = 1)
                .Build().ToList();

            var netImportList = Builder<NetImportDefinition>.CreateListOfSize(3)
                .Random(1)
                .With(c => c.ProfileId = 2)
                .Build().ToList();

            Mocker.GetMock<IMovieService>().Setup(c => c.GetAllMovies()).Returns(movieList);
            Mocker.GetMock<INetImportFactory>().Setup(c => c.All()).Returns(netImportList);

            Assert.Throws<ProfileInUseException>(() => Subject.Delete(2));

            Mocker.GetMock<IProfileRepository>().Verify(c => c.Delete(It.IsAny<int>()), Times.Never());
        }

        [Test]
        public void should_delete_profile_if_not_assigned_to_movie_or_list()
        {
            var movieList = Builder<Movie>.CreateListOfSize(3)
                                            .All()
                                            .With(c => c.ProfileId = 2)
                                            .Build().ToList();

            var netImportList = Builder<NetImportDefinition>.CreateListOfSize(3)
                                                            .All()
                                                            .With(c => c.ProfileId = 2)
                                                            .Build().ToList();

            Mocker.GetMock<IMovieService>().Setup(c => c.GetAllMovies()).Returns(movieList);
            Mocker.GetMock<INetImportFactory>().Setup(c => c.All()).Returns(netImportList);

            Subject.Delete(1);

            Mocker.GetMock<IProfileRepository>().Verify(c => c.Delete(1), Times.Once());
        }

        [Test]
        public void get_acceptable_languages_should_return_profile_language()
        {
            var profile = Builder<Profile>.CreateNew().With(c => c.Language = Language.German).Build();

            Mocker.GetMock<IProfileRepository>()
                  .Setup(s => s.Get(It.IsAny<int>()))
                  .Returns(profile);

            var languages = Subject.GetAcceptableLanguages(profile.Id);

            languages.Count.Should().Be(1);
            languages.Should().Contain(Language.German);
        }

        [Test]
        public void get_acceptable_languages_should_return_custom_format_positive_languages()
        {
            var profile = Builder<Profile>.CreateNew()
                .With(c => c.Language = Language.German)
                .Build();

            var customFormat1 = new CustomFormat("My Format 1", new LanguageSpecification { Value = (int)Language.English }) { Id = 1 };
            var customFormat2 = new CustomFormat("My Format 2", new LanguageSpecification { Value = (int)Language.French }) { Id = 2 };

            CustomFormatsFixture.GivenCustomFormats(customFormat1, customFormat2);

            profile.FormatItems = CustomFormatsFixture.GetSampleFormatItems(customFormat2.Name);

            Mocker.GetMock<IProfileRepository>()
                  .Setup(s => s.Get(It.IsAny<int>()))
                  .Returns(profile);

            var languages = Subject.GetAcceptableLanguages(profile.Id);

            languages.Count.Should().Be(2);
            languages.Should().Contain(Language.German);
            languages.Should().Contain(Language.French);
        }
    }
}
