using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Movies;
using System;
using System.Linq;

namespace NzbDrone.Core.Test.MovieTests.MovieRepositoryTests
{
    [TestFixture]

    public class MovieRepositoryFixture : DbTest<MovieRepository, Movie>
    {
        private IProfileRepository _profileRepository;

        [SetUp]
        public void Setup()
        {
            _profileRepository = Mocker.Resolve<ProfileRepository>();
            Mocker.SetConstant<IProfileRepository>(_profileRepository);
        }

        [Test]
        public void should_load_quality_profile()
        {
            var profile = new Profile
                {
                    Items = Qualities.QualityFixture.GetDefaultQualities(Quality.Bluray1080p, Quality.DVD, Quality.HDTV720p),
                    FormatItems = CustomFormat.CustomFormatsFixture.GetDefaultFormatItems(),
                    FormatCutoff = CustomFormats.CustomFormat.None.Id,
                    Cutoff = Quality.Bluray1080p.Id,
                    Name = "TestProfile"
                };

            _profileRepository.Insert(profile);

            var movie = Builder<Movie>.CreateNew().BuildNew();
            movie.ProfileId = profile.Id;

            Subject.Insert(movie);

            Subject.All().Single().Profile.Should().NotBeNull();
        }
    }
}
