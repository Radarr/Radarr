using System;
using System.Collections.Generic;
using System.Net;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.ImportLists.Spotify;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.MetadataSource.SkyHook.Resource;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ImportListTests
{
    [TestFixture]
    // the base import list class is abstract so use the followed artists one
    public class SpotifyMappingFixture : CoreTest<SpotifyFollowedArtists>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.SetConstant<ILidarrCloudRequestBuilder>(new LidarrCloudRequestBuilder());
            Mocker.SetConstant<IMetadataRequestBuilder>(Mocker.Resolve<MetadataRequestBuilder>());
        }

        [Test]
        public void map_artist_should_return_name_if_id_null()
        {
            var data = new SpotifyImportListItemInfo
            {
                Artist = "Adele"
            };

            Subject.MapArtistItem(data);

            data.Artist.Should().Be("Adele");
            data.ArtistMusicBrainzId.Should().BeNull();
            data.Album.Should().BeNull();
            data.AlbumMusicBrainzId.Should().BeNull();
        }

        [Test]
        public void map_artist_should_set_id_0_if_no_match()
        {
            Mocker.GetMock<IHttpClient>()
                .Setup(x => x.Get<ArtistResource>(It.IsAny<HttpRequest>()))
                .Returns<HttpRequest>((x) => new HttpResponse<ArtistResource>(new HttpResponse(x, new HttpHeader(), new byte[0], HttpStatusCode.NotFound)));

            var data = new SpotifyImportListItemInfo
            {
                Artist = "Adele",
                ArtistSpotifyId = "id"
            };

            Subject.MapArtistItem(data);
            data.ArtistMusicBrainzId.Should().Be("0");
        }

        [Test]
        public void map_artist_should_not_update_id_if_http_throws()
        {
            Mocker.GetMock<IHttpClient>()
                .Setup(x => x.Get<ArtistResource>(It.IsAny<HttpRequest>()))
                .Throws(new Exception("Dummy exception"));

            var data = new SpotifyImportListItemInfo
            {
                Artist = "Adele",
                ArtistSpotifyId = "id"
            };

            Subject.MapArtistItem(data);
            data.ArtistMusicBrainzId.Should().BeNull();

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void map_artist_should_work()
        {
            UseRealHttp();

            var data = new SpotifyImportListItemInfo
            {
                Artist = "Adele",
                ArtistSpotifyId = "4dpARuHxo51G3z768sgnrY"
            };

            Subject.MapArtistItem(data);
            data.Should().NotBeNull();
            data.Artist.Should().Be("Adele");
            data.ArtistMusicBrainzId.Should().Be("cc2c9c3c-b7bc-4b8b-84d8-4fbd8779e493");
            data.Album.Should().BeNull();
            data.AlbumMusicBrainzId.Should().BeNull();
        }

        [Test]
        public void map_album_should_return_name_if_uri_null()
        {
            var data = new SpotifyImportListItemInfo
            {
                Album = "25",
                Artist = "Adele"
            };

            Subject.MapAlbumItem(data);
            data.Should().NotBeNull();
            data.Artist.Should().Be("Adele");
            data.ArtistMusicBrainzId.Should().BeNull();
            data.Album.Should().Be("25");
            data.AlbumMusicBrainzId.Should().BeNull();
        }

        [Test]
        public void map_album_should_set_id_0_if_no_match()
        {
            Mocker.GetMock<IHttpClient>()
                .Setup(x => x.Get<AlbumResource>(It.IsAny<HttpRequest>()))
                .Returns<HttpRequest>((x) => new HttpResponse<AlbumResource>(new HttpResponse(x, new HttpHeader(), new byte[0], HttpStatusCode.NotFound)));

            var data = new SpotifyImportListItemInfo
            {
                Album = "25",
                AlbumSpotifyId = "id",
                Artist = "Adele"
            };

            Subject.MapAlbumItem(data);
            data.AlbumMusicBrainzId.Should().Be("0");
        }

        [Test]
        public void map_album_should_not_update_id_if_http_throws()
        {
            Mocker.GetMock<IHttpClient>()
                .Setup(x => x.Get<AlbumResource>(It.IsAny<HttpRequest>()))
                .Throws(new Exception("Dummy exception"));

            var data = new SpotifyImportListItemInfo
            {
                Album = "25",
                AlbumSpotifyId = "id",
                Artist = "Adele"
            };


            Subject.MapAlbumItem(data);
            data.Should().NotBeNull();
            data.Artist.Should().Be("Adele");
            data.ArtistMusicBrainzId.Should().BeNull();
            data.Album.Should().Be("25");
            data.AlbumMusicBrainzId.Should().BeNull();

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void map_album_should_work()
        {
            UseRealHttp();

            var data = new SpotifyImportListItemInfo
            {
                Album = "25",
                AlbumSpotifyId = "7uwTHXmFa1Ebi5flqBosig",
                Artist = "Adele"
            };

            Subject.MapAlbumItem(data);

            data.Should().NotBeNull();
            data.Artist.Should().Be("Adele");
            data.Album.Should().Be("25");
            data.AlbumMusicBrainzId.Should().Be("5537624c-3d2f-4f5c-8099-df916082c85c");
        }

        [Test]
        public void map_spotify_releases_should_only_map_album_id_for_album()
        {
            var data = new List<SpotifyImportListItemInfo>
            {
                new SpotifyImportListItemInfo
                {
                    Album = "25",
                    AlbumSpotifyId = "7uwTHXmFa1Ebi5flqBosig",
                    Artist = "Adele",
                    ArtistSpotifyId = "4dpARuHxo51G3z768sgnrY"
                }
            };

            var map = new List<SpotifyMap>
            {
                new SpotifyMap
                {
                    SpotifyId = "7uwTHXmFa1Ebi5flqBosig",
                    MusicbrainzId = "5537624c-3d2f-4f5c-8099-df916082c85c"
                },
                new SpotifyMap
                {
                    SpotifyId = "4dpARuHxo51G3z768sgnrY",
                    MusicbrainzId = "cc2c9c3c-b7bc-4b8b-84d8-4fbd8779e493"
                }
            };

            Mocker.GetMock<IHttpClient>()
                .Setup(x => x.Post<List<SpotifyMap>>(It.IsAny<HttpRequest>()))
                .Returns<HttpRequest>(r => new HttpResponse<List<SpotifyMap>>(new HttpResponse(r, new HttpHeader(), map.ToJson())));

            var result = Subject.MapSpotifyReleases(data);
            result[0].AlbumMusicBrainzId.Should().Be("5537624c-3d2f-4f5c-8099-df916082c85c");
            result[0].ArtistMusicBrainzId.Should().BeNull();
        }

        [Test]
        public void map_spotify_releases_should_map_artist_id_for_artist()
        {
            var data = new List<SpotifyImportListItemInfo>
            {
                new SpotifyImportListItemInfo
                {
                    Artist = "Adele",
                    ArtistSpotifyId = "4dpARuHxo51G3z768sgnrY"
                }
            };

            var map = new List<SpotifyMap>
            {
                new SpotifyMap
                {
                    SpotifyId = "7uwTHXmFa1Ebi5flqBosig",
                    MusicbrainzId = "5537624c-3d2f-4f5c-8099-df916082c85c"
                },
                new SpotifyMap
                {
                    SpotifyId = "4dpARuHxo51G3z768sgnrY",
                    MusicbrainzId = "cc2c9c3c-b7bc-4b8b-84d8-4fbd8779e493"
                }
            };

            Mocker.GetMock<IHttpClient>()
                .Setup(x => x.Post<List<SpotifyMap>>(It.IsAny<HttpRequest>()))
                .Returns<HttpRequest>(r => new HttpResponse<List<SpotifyMap>>(new HttpResponse(r, new HttpHeader(), map.ToJson())));

            var result = Subject.MapSpotifyReleases(data);
            result[0].ArtistMusicBrainzId.Should().Be("cc2c9c3c-b7bc-4b8b-84d8-4fbd8779e493");
        }

        [Test]
        public void map_spotify_releases_should_drop_not_found()
        {
            var data = new List<SpotifyImportListItemInfo>
            {
                new SpotifyImportListItemInfo
                {
                    Album = "25",
                    AlbumSpotifyId = "7uwTHXmFa1Ebi5flqBosig",
                    Artist = "Adele"
                }
            };

            var map = new List<SpotifyMap>
            {
                new SpotifyMap
                {
                    SpotifyId = "7uwTHXmFa1Ebi5flqBosig",
                    MusicbrainzId = "0"
                }
            };

            Mocker.GetMock<IHttpClient>()
                .Setup(x => x.Post<List<SpotifyMap>>(It.IsAny<HttpRequest>()))
                .Returns<HttpRequest>(r => new HttpResponse<List<SpotifyMap>>(new HttpResponse(r, new HttpHeader(), map.ToJson())));

            var result = Subject.MapSpotifyReleases(data);
            result.Should().BeEmpty();
        }

        [Test]
        public void map_spotify_releases_should_catch_exception_from_api()
        {
            var data = new List<SpotifyImportListItemInfo>
            {
                new SpotifyImportListItemInfo
                {
                    Album = "25",
                    AlbumSpotifyId = "7uwTHXmFa1Ebi5flqBosig",
                    Artist = "Adele"
                }
            };

            Mocker.GetMock<IHttpClient>()
                .Setup(x => x.Post<List<SpotifyMap>>(It.IsAny<HttpRequest>()))
                .Throws(new Exception("Dummy exception"));

            Mocker.GetMock<IHttpClient>()
                .Setup(x => x.Get<AlbumResource>(It.IsAny<HttpRequest>()))
                .Throws(new Exception("Dummy exception"));


            var result = Subject.MapSpotifyReleases(data);
            result.Should().NotBeNull();
            ExceptionVerification.ExpectedErrors(2);
        }

        [Test]
        public void map_spotify_releases_should_cope_with_duplicate_spotify_ids()
        {
            var data = new List<SpotifyImportListItemInfo>
            {
                new SpotifyImportListItemInfo
                {
                    Album = "25",
                    AlbumSpotifyId = "7uwTHXmFa1Ebi5flqBosig",
                    Artist = "Adele"
                },
                new SpotifyImportListItemInfo
                {
                    Album = "25",
                    AlbumSpotifyId = "7uwTHXmFa1Ebi5flqBosig",
                    Artist = "Adele"
                }
            };

            var map = new List<SpotifyMap>
            {
                new SpotifyMap
                {
                    SpotifyId = "7uwTHXmFa1Ebi5flqBosig",
                    MusicbrainzId = "5537624c-3d2f-4f5c-8099-df916082c85c"
                }
            };

            Mocker.GetMock<IHttpClient>()
                .Setup(x => x.Post<List<SpotifyMap>>(It.IsAny<HttpRequest>()))
                .Returns<HttpRequest>(r => new HttpResponse<List<SpotifyMap>>(new HttpResponse(r, new HttpHeader(), map.ToJson())));

            var result = Subject.MapSpotifyReleases(data);
            result.Should().HaveCount(2);
            result[0].AlbumMusicBrainzId.Should().Be("5537624c-3d2f-4f5c-8099-df916082c85c");
            result[1].AlbumMusicBrainzId.Should().Be("5537624c-3d2f-4f5c-8099-df916082c85c");
        }
    }
}
