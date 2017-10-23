﻿using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Profiles
{
    [TestFixture]

    public class ProfileServiceFixture : CoreTest<ProfileService>
    {
        [Test]
        public void init_should_add_default_profiles()
        {
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


            Mocker.GetMock<IMovieService>().Setup(c => c.GetAllMovies()).Returns(movieList);

            Assert.Throws<ProfileInUseException>(() => Subject.Delete(2));

            Mocker.GetMock<IProfileRepository>().Verify(c => c.Delete(It.IsAny<int>()), Times.Never());

        }


        [Test]
        public void should_delete_profile_if_not_assigned_to_movie()
        {
            var movieList = Builder<Movie>.CreateListOfSize(3)
                                            .All()
                                            .With(c => c.ProfileId = 2)
                                            .Build().ToList();


            Mocker.GetMock<IMovieService>().Setup(c => c.GetAllMovies()).Returns(movieList);

            Subject.Delete(1);

            Mocker.GetMock<IProfileRepository>().Verify(c => c.Delete(1), Times.Once());
        }
    }
}