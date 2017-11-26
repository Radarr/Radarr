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
        }

        [TestCase("f59c5520-5f46-4d2c-b2c4-822eabf53419", "Linkin Park")]
        [TestCase("66c662b6-6e2f-4930-8610-912e24c63ed1", "AC/DC")]
        public void should_be_able_to_get_artist_detail(string mbId, string name)
        {
            var details = Subject.GetArtistInfo(mbId, 0);

            ValidateArtist(details.Item1);
            ValidateAlbums(details.Item2);

            details.Item1.Name.Should().Be(name);
        }

        [Test]
        public void getting_details_of_invalid_artist()
        {
            Assert.Throws<ArtistNotFoundException>(() => Subject.GetArtistInfo("aaaaaa-aaa-aaaa-aaaa", 0));
        }

        [Test]
        public void should_not_have_period_at_start_of_name_slug()
        {
            var details = Subject.GetArtistInfo("b6db95cd-88d9-492f-bbf6-a34e0e89b2e5", 0);

            details.Item1.NameSlug.Should().Be("dothack");
        }

        private void ValidateArtist(Artist artist)
        {
            artist.Should().NotBeNull();
            artist.Name.Should().NotBeNullOrWhiteSpace();
            artist.CleanName.Should().Be(Parser.Parser.CleanArtistName(artist.Name));
            artist.SortName.Should().Be(Parser.Parser.NormalizeTitle(artist.Name));
            artist.Overview.Should().NotBeNullOrWhiteSpace();
            artist.Images.Should().NotBeEmpty();
            artist.NameSlug.Should().NotBeNullOrWhiteSpace();
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
