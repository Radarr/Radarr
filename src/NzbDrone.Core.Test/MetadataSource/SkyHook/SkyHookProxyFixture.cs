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
using NzbDrone.Core.MetadataSource.SkyHook.Resource;

namespace NzbDrone.Core.Test.MetadataSource.SkyHook
{
    [TestFixture]
    [IntegrationTest]
    public class SkyHookProxyFixture : CoreTest<SkyHookProxy>
    {
        private MetadataProfile _metadataProfile;

        [SetUp]
        public void Setup()
        {
            UseRealHttp();

            _metadataProfile = new MetadataProfile
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
                .Setup(s => s.Get(It.IsAny<int>()))
                .Returns(_metadataProfile);

            Mocker.GetMock<IMetadataProfileService>()
                .Setup(s => s.Exists(It.IsAny<int>()))
                .Returns(true);
        }
        
        public List<AlbumResource> GivenExampleAlbums()
        {
            var result = new List<AlbumResource>();
            
            foreach (var primaryType in PrimaryAlbumType.All)
            {
                foreach (var secondaryType in SecondaryAlbumType.All)
                {
                    var secondaryTypes = secondaryType.Name == "Studio" ? new List<string>() : new List<string> { secondaryType.Name };
                    foreach (var releaseStatus in ReleaseStatus.All)
                    {
                        var releaseStatuses = new List<string> { releaseStatus.Name };
                        result.Add(new AlbumResource {
                                Type = primaryType.Name,
                                SecondaryTypes = secondaryTypes,
                                ReleaseStatuses = releaseStatuses
                            });
                    }
                }
            }
            
            return result;
        }

        [TestCase("f59c5520-5f46-4d2c-b2c4-822eabf53419", "Linkin Park")]
        [TestCase("66c662b6-6e2f-4930-8610-912e24c63ed1", "AC/DC")]
        public void should_be_able_to_get_artist_detail(string mbId, string name)
        {
            var details = Subject.GetArtistInfo(mbId, 1);

            ValidateArtist(details);
            ValidateAlbums(details.Albums.Value, true);

            details.Name.Should().Be(name);
        }

        [TestCaseSource(typeof(PrimaryAlbumType), "All")]
        public void should_filter_albums_by_primary_release_type(PrimaryAlbumType type)
        {
            _metadataProfile.PrimaryAlbumTypes = new List<ProfilePrimaryAlbumTypeItem> {
                    new ProfilePrimaryAlbumTypeItem
                    {
                        PrimaryAlbumType = type,
                        Allowed = true
                    }
            };


            var albums = GivenExampleAlbums();
            Subject.FilterAlbums(albums, 1).Select(x => x.Type).Distinct()
                   .Should().BeEquivalentTo(new List<string> { type.Name });
        }
        
        [TestCaseSource(typeof(SecondaryAlbumType), "All")]
        public void should_filter_albums_by_secondary_release_type(SecondaryAlbumType type)
        {
            _metadataProfile.SecondaryAlbumTypes = new List<ProfileSecondaryAlbumTypeItem> {
                    new ProfileSecondaryAlbumTypeItem
                    {
                        SecondaryAlbumType = type,
                        Allowed = true
                    }
            };

            var albums = GivenExampleAlbums();
            var filtered = Subject.FilterAlbums(albums, 1);
            TestLogger.Debug(filtered.Count());
            
            filtered.SelectMany(x => x.SecondaryTypes.Select(SkyHookProxy.MapSecondaryTypes))
                    .Select(x => x.Name)
                    .Distinct()
                    .Should().BeEquivalentTo(type.Name == "Studio" ? new List<string>() : new List<string> { type.Name });
        }

        [TestCaseSource(typeof(ReleaseStatus), "All")]
        public void should_filter_albums_by_release_status(ReleaseStatus type)
        {
            _metadataProfile.ReleaseStatuses = new List<ProfileReleaseStatusItem> {
                    new ProfileReleaseStatusItem
                    {
                        ReleaseStatus = type,
                        Allowed = true
                    }
            };

            var albums = GivenExampleAlbums();
            Subject.FilterAlbums(albums, 1).SelectMany(x => x.ReleaseStatuses).Distinct()
                   .Should().BeEquivalentTo(new List<string> { type.Name });
        }
        
        [TestCase("12fa3845-7c62-36e5-a8da-8be137155a72", "Hysteria")]
        public void should_be_able_to_get_album_detail(string mbId, string name)
        {
            var details = Subject.GetAlbumInfo(mbId);
            
            ValidateAlbums(new List<Album> {details.Item2});

            details.Item2.Title.Should().Be(name);
        }

        [TestCase("12fa3845-7c62-36e5-a8da-8be137155a72", "3c186b52-ca73-46a3-a8e6-04559bfbb581",1, 13, "Hysteria")]
        [TestCase("12fa3845-7c62-36e5-a8da-8be137155a72", "dee9ca6f-4f84-4359-82a9-b75a37ffc316",2, 27,"Hysteria")]
        public void should_be_able_to_get_album_detail_with_release(string mbId, string release, int mediaCount, int trackCount, string name)
        {
            var details = Subject.GetAlbumInfo(mbId);

            ValidateAlbums(new List<Album> { details.Item2 });

            details.Item2.AlbumReleases.Value.Single(r => r.ForeignReleaseId == release).Media.Count.Should().Be(mediaCount);
            details.Item2.AlbumReleases.Value.Single(r => r.ForeignReleaseId == release).Tracks.Value.Count.Should().Be(trackCount);
            details.Item2.Title.Should().Be(name);
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
            Assert.Throws<AlbumNotFoundException>(() => Subject.GetAlbumInfo("66c66aaa-6e2f-4930-8610-912e24c63ed1"));
        }

        [Test]
        public void getting_details_of_invalid_guid_for_album()
        {
            Assert.Throws<BadRequestException>(() => Subject.GetAlbumInfo("66c66aaa-6e2f-4930-aaaaaa"));
        }

        private void ValidateArtist(Artist artist)
        {
            artist.Should().NotBeNull();
            artist.Name.Should().NotBeNullOrWhiteSpace();
            artist.CleanName.Should().Be(Parser.Parser.CleanArtistName(artist.Name));
            artist.SortName.Should().Be(Parser.Parser.NormalizeTitle(artist.Name));
            artist.Metadata.Value.Overview.Should().NotBeNullOrWhiteSpace();
            artist.Metadata.Value.Images.Should().NotBeEmpty();
            artist.ForeignArtistId.Should().NotBeNullOrWhiteSpace();
        }

        private void ValidateAlbums(List<Album> albums, bool idOnly = false)
        {
            albums.Should().NotBeEmpty();

            foreach (var album in albums)
            {
                album.ForeignAlbumId.Should().NotBeNullOrWhiteSpace();
                if (!idOnly)
                {
                    ValidateAlbum(album);
                }

            }
            
            //if atleast one album has title it means parse it working.
            if (!idOnly)
            {
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
