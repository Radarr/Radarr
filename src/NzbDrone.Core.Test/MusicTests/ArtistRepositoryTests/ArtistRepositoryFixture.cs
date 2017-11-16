using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using System.Linq;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;
using NzbDrone.Core.Profiles.Languages;

namespace NzbDrone.Core.Test.MusicTests.ArtistRepositoryTests
{
    [TestFixture]

    public class ArtistRepositoryFixture : DbTest<ArtistRepository, Artist>
    {
        [Test]
        public void should_lazyload_quality_profile()
        {
            var profile = new Profile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities(Quality.FLAC, Quality.MP3_192, Quality.MP3_320),

                Cutoff = Quality.FLAC.Id,
                Name = "TestProfile"
            };

            var langProfile = new LanguageProfile
            {
                Name = "TestProfile",
                Languages = Languages.LanguageFixture.GetDefaultLanguages(Language.English),
                Cutoff = Language.English
            };


            Mocker.Resolve<ProfileRepository>().Insert(profile);
            Mocker.Resolve<LanguageProfileRepository>().Insert(langProfile);

            var series = Builder<Artist>.CreateNew().BuildNew();
            series.ProfileId = profile.Id;
            series.LanguageProfileId = langProfile.Id;

            Subject.Insert(series);


            StoredModel.Profile.Should().NotBeNull();
            StoredModel.LanguageProfile.Should().NotBeNull();


        }
    }
}
