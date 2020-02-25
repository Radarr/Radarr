using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.CustomFormats;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Profiles
{
    [TestFixture]
    public class ProfileRepositoryFixture : DbTest<ProfileRepository, Profile>
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void should_be_able_to_read_and_write()
        {
            var profile = new Profile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities(Quality.Bluray1080p, Quality.DVD, Quality.HDTV720p),
                MinFormatScore = 0,
                FormatItems = CustomFormatsFixture.GetDefaultFormatItems(),
                Cutoff = Quality.Bluray1080p.Id,
                Name = "TestProfile"
            };

            Subject.Insert(profile);

            StoredModel.Name.Should().Be(profile.Name);
            StoredModel.Cutoff.Should().Be(profile.Cutoff);

            StoredModel.Items.Should().Equal(profile.Items, (a, b) => a.Quality == b.Quality && a.Allowed == b.Allowed);
        }
    }
}
