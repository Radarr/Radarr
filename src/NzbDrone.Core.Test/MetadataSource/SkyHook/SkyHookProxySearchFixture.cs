using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.MetadataSource.SkyHook;
using NzbDrone.Core.Music;
using NzbDrone.Core.Profiles.Metadata;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MetadataSource.SkyHook
{
    [TestFixture]
    public class SkyHookProxySearchFixture : CoreTest<SkyHookProxy>
    {
        [SetUp]
        public void Setup()
        {
            UseRealHttp();

            var metadataProfile = new MetadataProfile
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
                .Returns(new List<MetadataProfile> { metadataProfile });

            Mocker.GetMock<IMetadataProfileService>()
                .Setup(s => s.Get(It.IsAny<int>()))
                .Returns(metadataProfile);
        }

        [TestCase("Coldplay", "Coldplay")]
        [TestCase("Avenged Sevenfold", "Avenged Sevenfold")]
        [TestCase("3OH!3", "3OH!3")]
        [TestCase("The Academy Is...", "The Academy Is…")]
        [TestCase("readarr:f59c5520-5f46-4d2c-b2c4-822eabf53419", "Linkin Park")]
        [TestCase("readarrid:f59c5520-5f46-4d2c-b2c4-822eabf53419", "Linkin Park")]
        [TestCase("readarrid: f59c5520-5f46-4d2c-b2c4-822eabf53419 ", "Linkin Park")]
        public void successful_artist_search(string title, string expected)
        {
            var result = Subject.SearchForNewArtist(title);

            result.Should().NotBeEmpty();

            result[0].Name.Should().Be(expected);

            ExceptionVerification.IgnoreWarns();
        }

        [TestCase("Evolve", "Imagine Dragons", "Evolve")]
        [TestCase("Hysteria", null, "Hysteria")]
        [TestCase("readarr:d77df681-b779-3d6d-b66a-3bfd15985e3e", null, "Pyromania")]
        [TestCase("readarr: d77df681-b779-3d6d-b66a-3bfd15985e3e", null, "Pyromania")]
        [TestCase("readarrid:d77df681-b779-3d6d-b66a-3bfd15985e3e", null, "Pyromania")]
        public void successful_album_search(string title, string artist, string expected)
        {
            var result = Subject.SearchForNewAlbum(title, artist);

            result.Should().NotBeEmpty();

            result[0].Title.Should().Be(expected);

            ExceptionVerification.IgnoreWarns();
        }

        [TestCase("readarrid:")]
        [TestCase("readarrid: 99999999999999999999")]
        [TestCase("readarrid: 0")]
        [TestCase("readarrid: -12")]
        [TestCase("readarrid:289578")]
        [TestCase("adjalkwdjkalwdjklawjdlKAJD")]
        public void no_artist_search_result(string term)
        {
            var result = Subject.SearchForNewArtist(term);
            result.Should().BeEmpty();

            ExceptionVerification.IgnoreWarns();
        }

        [TestCase("Eminem", 0, typeof(Artist), "Eminem")]
        [TestCase("Eminem Kamikaze", 0, typeof(Artist), "Eminem")]
        [TestCase("Eminem Kamikaze", 1, typeof(Album), "Kamikaze")]
        public void successful_combined_search(string query, int position, Type resultType, string expected)
        {
            var result = Subject.SearchForNewEntity(query);
            result.Should().NotBeEmpty();
            result[position].GetType().Should().Be(resultType);

            if (resultType == typeof(Artist))
            {
                var cast = result[position] as Artist;
                cast.Should().NotBeNull();
                cast.Name.Should().Be(expected);
            }
            else
            {
                var cast = result[position] as Album;
                cast.Should().NotBeNull();
                cast.Title.Should().Be(expected);
            }
        }
    }
}
