using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.TvTests.SeriesRepositoryTests
{
    [TestFixture]

    public class SeriesRepositoryFixture : DbTest<SeriesRepository, Series>
    {
        [Test]
        public void should_lazyload_quality_profile()
        {
            var profile = new Profile
                {
                    Items = Qualities.QualityFixture.GetDefaultQualities(Quality.MP3320, Quality.MP3256, Quality.MP3192),

                    Cutoff = Quality.MP3320,
                    Name = "TestProfile"
                };


            Mocker.Resolve<ProfileRepository>().Insert(profile);

            var series = Builder<Series>.CreateNew().BuildNew();
            series.ProfileId = profile.Id;

            Subject.Insert(series);


            StoredModel.Profile.Should().NotBeNull();


        }
    }
}