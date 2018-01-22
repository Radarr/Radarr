using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaCover;
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

        [Test]
        public void getting_details_of_invalid_artist()
        {
            Assert.Throws<BadRequestException>(() => Subject.GetArtistInfo("aaaaaa-aaa-aaaa-aaaa", 1));
        }

        private void ValidateArtist(Artist artist)
        {
            artist.Should().NotBeNull();
            artist.Name.Should().NotBeNullOrWhiteSpace();
            artist.CleanName.Should().Be(Parser.Parser.CleanArtistName(artist.Name));
            artist.SortName.Should().Be(Parser.Parser.NormalizeTitle(artist.Name));
            artist.Overview.Should().NotBeNullOrWhiteSpace();
            artist.Images.Should().NotBeEmpty();
            //series.TvRageId.Should().BeGreaterThan(0);
            artist.ForeignArtistId.Should().NotBeNullOrWhiteSpace();
        }

        private void ValidateAlbums(List<Album> albums)
        {
            albums.Should().NotBeEmpty();

            foreach (var episode in albums)
            {
                ValidateAlbum(episode);

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
