using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MetadataSource.SkyHook;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;
using NzbDrone.Test.Common.Categories;
using Moq;
using NzbDrone.Core.Profiles.Metadata;

namespace NzbDrone.Core.Test.MetadataSource.SkyHook
{
    [TestFixture]
    [IntegrationTest]
    public class SkyHookProxyFixture : CoreTest<SkyHookProxy>
    {
        [SetUp]
        public void Setup()
        {
            UseRealHttp();

            var _metadataProfile = new MetadataProfile
            {
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
            };

            Mocker.GetMock<IMetadataProfileService>()
                .Setup(s => s.Get(It.IsAny<int>()))
                .Returns(_metadataProfile);

            Mocker.GetMock<IMetadataProfileService>()
                .Setup(s => s.Exists(It.IsAny<int>()))
                .Returns(true);
        }

        [TestCase("f59c5520-5f46-4d2c-b2c4-822eabf53419", "Linkin Park")]
        [TestCase("66c662b6-6e2f-4930-8610-912e24c63ed1", "AC/DC")]
        public void should_be_able_to_get_artist_detail(string mbId, string name)
        {
            var details = Subject.GetArtistInfo(mbId, 1);

            ValidateArtist(details.Item1);
            ValidateAlbums(details.Item2);

            details.Item1.Name.Should().Be(name);
        }
        
        [TestCase("12fa3845-7c62-36e5-a8da-8be137155a72", null, "Hysteria")]
        public void should_be_able_to_get_album_detail(string mbId, string release, string name)
        {
            var details = Subject.GetAlbumInfo(mbId, release);
            
            ValidateAlbums(new List<Album> {details.Item1});

            details.Item1.Title.Should().Be(name);
        }

        [TestCase("12fa3845-7c62-36e5-a8da-8be137155a72", "3c186b52-ca73-46a3-a8e6-04559bfbb581",1, 13, "Hysteria")]
        [TestCase("12fa3845-7c62-36e5-a8da-8be137155a72", "dee9ca6f-4f84-4359-82a9-b75a37ffc316",2, 27,"Hysteria")]
        public void should_be_able_to_get_album_detail_with_release(string mbId, string release, int mediaCount, int trackCount, string name)
        {
            var details = Subject.GetAlbumInfo(mbId, release);

            ValidateAlbums(new List<Album> { details.Item1 });

            details.Item1.Media.Count.Should().Be(mediaCount);
            details.Item2.Count.Should().Be(trackCount);

            details.Item1.Title.Should().Be(name);
        }

        [Test]
        public void getting_details_of_invalid_artist()
        {
            Assert.Throws<ArtistNotFoundException>(() => Subject.GetArtistInfo("66c66aaa-6e2f-4930-8610-912e24c63ed1", 1));
        }

        [Test]
        public void getting_details_of_invalid_guid_for_artist()
        {
            Assert.Throws<BadRequestException>(() => Subject.GetArtistInfo("66c66aaa-6e2f-4930-aaaaaa", 1));
        }

        [Test]
        public void getting_details_of_invalid_album()
        {
            Assert.Throws<AlbumNotFoundException>(() => Subject.GetAlbumInfo("66c66aaa-6e2f-4930-8610-912e24c63ed1",null));
        }

        [Test]
        public void getting_details_of_invalid_guid_for_album()
        {
            Assert.Throws<BadRequestException>(() => Subject.GetAlbumInfo("66c66aaa-6e2f-4930-aaaaaa", null));
        }

        private void ValidateArtist(Artist artist)
        {
            artist.Should().NotBeNull();
            artist.Name.Should().NotBeNullOrWhiteSpace();
            artist.CleanName.Should().Be(Parser.Parser.CleanArtistName(artist.Name));
            artist.SortName.Should().Be(Parser.Parser.NormalizeTitle(artist.Name));
            artist.Overview.Should().NotBeNullOrWhiteSpace();
            artist.Images.Should().NotBeEmpty();
            artist.ForeignArtistId.Should().NotBeNullOrWhiteSpace();
        }

        private void ValidateAlbums(List<Album> albums)
        {
            albums.Should().NotBeEmpty();

            foreach (var album in albums)
            {
                ValidateAlbum(album);

                //if atleast one album has title it means parse it working.
                albums.Should().Contain(c => !string.IsNullOrWhiteSpace(c.Title));
            }
        }

        private void ValidateAlbum(Album album)
        {
            album.Should().NotBeNull();
            
            album.Title.Should().NotBeNullOrWhiteSpace();
            album.AlbumType.Should().NotBeNullOrWhiteSpace();

            album.Should().NotBeNull();

            if (album.ReleaseDate.HasValue)
            {
                album.ReleaseDate.Value.Kind.Should().Be(DateTimeKind.Utc);
            }
        }
    }
}
