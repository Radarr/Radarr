using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Test.Qualities;

namespace NzbDrone.Core.Test.MovieTests.MovieRepositoryTests
{
    [TestFixture]

    public class MovieRepositoryFixture : DbTest<MovieRepository, Movie>
    {
        [SetUp]
        public void Setup()
        {
            QualityDefinitionServiceFixture.SetupDefaultDefinitions();
        }

        [Test]
        public void should_lazyload_quality_profile()
        {
            var profile = new Profile
                {
                    Items = Qualities.QualityFixture.GetDefaultQualities(Quality.Bluray1080p, Quality.DVD, Quality.HDTV720p),

                    Cutoff = QualityWrapper.Dynamic.Bluray1080p,
                    Name = "TestProfile"
                };


            Mocker.Resolve<ProfileRepository>().Insert(profile);

            var movie = Builder<Movie>.CreateNew().BuildNew();
            movie.ProfileId = profile.Id;

            Subject.Insert(movie);


            StoredModel.Profile.Should().NotBeNull();


        }
    }
}
