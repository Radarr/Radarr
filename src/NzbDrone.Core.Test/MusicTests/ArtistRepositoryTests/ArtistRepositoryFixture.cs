using System.Collections.Generic;
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
using NzbDrone.Core.Profiles.Metadata;

namespace NzbDrone.Core.Test.MusicTests.ArtistRepositoryTests
{
    [TestFixture]

    public class ArtistRepositoryFixture : DbTest<ArtistRepository, Artist>
    {
        [Test]
        public void should_lazyload_profiles()
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

            var metaProfile = new MetadataProfile
            {
                Name = "TestProfile",
                PrimaryAlbumTypes = new List<ProfilePrimaryAlbumTypeItem>(),
                SecondaryAlbumTypes = new List<ProfileSecondaryAlbumTypeItem>(),
                ReleaseStatuses = new List<ProfileReleaseStatusItem>()
            };


            Mocker.Resolve<ProfileRepository>().Insert(profile);
            Mocker.Resolve<LanguageProfileRepository>().Insert(langProfile);
            Mocker.Resolve<MetadataProfileRepository>().Insert(metaProfile);

            var artist = Builder<Artist>.CreateNew().BuildNew();
            artist.ProfileId = profile.Id;
            artist.LanguageProfileId = langProfile.Id;
            artist.MetadataProfileId = metaProfile.Id;

            Subject.Insert(artist);


            StoredModel.Profile.Should().NotBeNull();
            StoredModel.LanguageProfile.Should().NotBeNull();
            StoredModel.MetadataProfile.Should().NotBeNull();

        }
    }
}
