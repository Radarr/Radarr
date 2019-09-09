using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MetadataSource.SkyHook;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.Categories;
using Moq;
using NzbDrone.Core.Profiles.Metadata;
using NzbDrone.Core.Music;
using System.Collections.Generic;

namespace NzbDrone.Core.Test.MetadataSource.SkyHook
{
    [TestFixture]
    [IntegrationTest]
    public class SkyHookProxySearchFixture : CoreTest<SkyHookProxy>
    {
        [SetUp]
        public void Setup()
        {
            UseRealHttp();

            var _metadataProfile = new MetadataProfile
            {
                Id = 1,
                PrimaryAlbumTypes = new List<ProfilePrimaryAlbumTypeItem>
                {
                    new ProfilePrimaryAlbumTypeItem
                    {
                        PrimaryAlbumType = PrimaryAlbumType.Album,
                        Allowed = true

                    }
                },
                SecondaryAlbumTypes = new List<ProfileSecondaryAlbumTypeItem>
                {
                    new ProfileSecondaryAlbumTypeItem()
                    {
                        SecondaryAlbumType = SecondaryAlbumType.Studio,
                        Allowed = true
                    }
                },
                ReleaseStatuses = new List<ProfileReleaseStatusItem>
                {
                    new ProfileReleaseStatusItem
                    {
                        ReleaseStatus = ReleaseStatus.Official,
                        Allowed = true
                    }
                }
            };

            Mocker.GetMock<IMetadataProfileService>()
                .Setup(s => s.All())
                .Returns(new List<MetadataProfile>{_metadataProfile});

            Mocker.GetMock<IMetadataProfileService>()
                .Setup(s => s.Get(It.IsAny<int>()))
                .Returns(_metadataProfile);
        }

        [TestCase("Coldplay", "Coldplay")]
        [TestCase("Avenged Sevenfold", "Avenged Sevenfold")]
        [TestCase("3OH!3", "3OH!3")]
        [TestCase("The Academy Is...", "The Academy Is…")]
        [TestCase("lidarr:f59c5520-5f46-4d2c-b2c4-822eabf53419", "Linkin Park")]
        [TestCase("lidarrid:f59c5520-5f46-4d2c-b2c4-822eabf53419", "Linkin Park")]
        [TestCase("lidarrid: f59c5520-5f46-4d2c-b2c4-822eabf53419 ", "Linkin Park")]
        public void successful_artist_search(string title, string expected)
        {
            var result = Subject.SearchForNewArtist(title);

            result.Should().NotBeEmpty();

            result[0].Name.Should().Be(expected);

            ExceptionVerification.IgnoreWarns();
        }


        [TestCase("Evolve", "Imagine Dragons", "Evolve")]
        [TestCase("Hysteria", null, "Hysteria")]
        [TestCase("lidarr:d77df681-b779-3d6d-b66a-3bfd15985e3e", null, "Pyromania")]
        [TestCase("lidarr: d77df681-b779-3d6d-b66a-3bfd15985e3e", null, "Pyromania")]
        [TestCase("lidarrid:d77df681-b779-3d6d-b66a-3bfd15985e3e", null, "Pyromania")]
        public void successful_album_search(string title, string artist, string expected)
        {
            var result = Subject.SearchForNewAlbum(title, artist);

            result.Should().NotBeEmpty();

            result[0].Title.Should().Be(expected);

            ExceptionVerification.IgnoreWarns();
        }

        [TestCase("lidarrid:")]
        [TestCase("lidarrid: 99999999999999999999")]
        [TestCase("lidarrid: 0")]
        [TestCase("lidarrid: -12")]
        [TestCase("lidarrid:289578")]
        [TestCase("adjalkwdjkalwdjklawjdlKAJD")]
        public void no_artist_search_result(string term)
        {
            var result = Subject.SearchForNewArtist(term);
            result.Should().BeEmpty();
            
            ExceptionVerification.IgnoreWarns();
        }
    }
}
