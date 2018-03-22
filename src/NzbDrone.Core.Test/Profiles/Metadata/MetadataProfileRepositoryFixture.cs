using FluentAssertions;
using System.Linq;
using NUnit.Framework;
using NzbDrone.Core.Music;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Profiles.Metadata;

namespace NzbDrone.Core.Test.Profiles.Metadata
{
    [TestFixture]
    public class MetadataProfileRepositoryFixture : DbTest<MetadataProfileRepository, MetadataProfile>
    {
        [Test]
        public void should_be_able_to_read_and_write()
        {
            var profile = new MetadataProfile
            {
                PrimaryAlbumTypes = PrimaryAlbumType.All.OrderByDescending(l => l.Name).Select(l => new ProfilePrimaryAlbumTypeItem
                {
                    PrimaryAlbumType = l,
                    Allowed = l == PrimaryAlbumType.Album
                }).ToList(),

                SecondaryAlbumTypes = SecondaryAlbumType.All.OrderByDescending(l => l.Name).Select(l => new ProfileSecondaryAlbumTypeItem
                {
                    SecondaryAlbumType = l,
                    Allowed = l == SecondaryAlbumType.Studio
                }).ToList(),

                ReleaseStatuses = ReleaseStatus.All.OrderByDescending(l => l.Name).Select(l => new ProfileReleaseStatusItem
                {
                    ReleaseStatus = l,
                    Allowed = l == ReleaseStatus.Official
                }).ToList(),

                Name = "TestProfile"
            };

            Subject.Insert(profile);

            StoredModel.Name.Should().Be(profile.Name);

            StoredModel.PrimaryAlbumTypes.Should().Equal(profile.PrimaryAlbumTypes, (a, b) => a.PrimaryAlbumType == b.PrimaryAlbumType && a.Allowed == b.Allowed);
            StoredModel.SecondaryAlbumTypes.Should().Equal(profile.SecondaryAlbumTypes, (a, b) => a.SecondaryAlbumType == b.SecondaryAlbumType && a.Allowed == b.Allowed);
            StoredModel.ReleaseStatuses.Should().Equal(profile.ReleaseStatuses, (a, b) => a.ReleaseStatus == b.ReleaseStatus && a.Allowed == b.Allowed);
        }
    }
}
