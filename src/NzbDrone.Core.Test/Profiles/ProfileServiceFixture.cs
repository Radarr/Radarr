using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Movies;
using NzbDrone.Core.NetImport;
using NzbDrone.Core.Profiles;
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
                .Returns(new List<CustomFormats.CustomFormat>());

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
    }
}
