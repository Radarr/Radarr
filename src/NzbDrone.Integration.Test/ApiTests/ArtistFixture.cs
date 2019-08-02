using FluentAssertions;
using NUnit.Framework;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class ArtistFixture : IntegrationTest
    {
        [Test, Order(0)]
        public void add_artist_with_tags_should_store_them()
        {
            EnsureNoArtist("f59c5520-5f46-4d2c-b2c4-822eabf53419", "Linkin Park");
            var tag = EnsureTag("abc");

            var artist = Artist.Lookup("lidarr:f59c5520-5f46-4d2c-b2c4-822eabf53419").Single();

            artist.QualityProfileId = 1;
            artist.MetadataProfileId = 1;
            artist.Path = Path.Combine(ArtistRootFolder, artist.ArtistName);
            artist.Tags = new HashSet<int>();
            artist.Tags.Add(tag.Id);

            var result = Artist.Post(artist);

            result.Should().NotBeNull();
            result.Tags.Should().Equal(tag.Id);
        }

        [Test, Order(0)]
        public void add_artist_without_profileid_should_return_badrequest()
        {
            EnsureNoArtist("f59c5520-5f46-4d2c-b2c4-822eabf53419", "Linkin Park");

            var artist = Artist.Lookup("lidarr:f59c5520-5f46-4d2c-b2c4-822eabf53419").Single();

            artist.Path = Path.Combine(ArtistRootFolder, artist.ArtistName);

            Artist.InvalidPost(artist);
        }

        [Test, Order(0)]
        public void add_artist_without_path_should_return_badrequest()
        {
            EnsureNoArtist("f59c5520-5f46-4d2c-b2c4-822eabf53419", "Linkin Park");

            var artist = Artist.Lookup("lidarr:f59c5520-5f46-4d2c-b2c4-822eabf53419").Single();

            artist.QualityProfileId = 1;

            Artist.InvalidPost(artist);
        }

        [Test, Order(1)]
        public void add_artist()
        {
            EnsureNoArtist("f59c5520-5f46-4d2c-b2c4-822eabf53419", "Linkin Park");

            var artist = Artist.Lookup("lidarr:f59c5520-5f46-4d2c-b2c4-822eabf53419").Single();

            artist.QualityProfileId = 1;
            artist.MetadataProfileId = 1;
            artist.Path = Path.Combine(ArtistRootFolder, artist.ArtistName);

            var result = Artist.Post(artist);

            result.Should().NotBeNull();
            result.Id.Should().NotBe(0);
            result.QualityProfileId.Should().Be(1);
            result.MetadataProfileId.Should().Be(1);
            result.Path.Should().Be(Path.Combine(ArtistRootFolder, artist.ArtistName));
        }


        [Test, Order(2)]
        public void get_all_artist()
        {
            EnsureArtist("8ac6cc32-8ddf-43b1-9ac4-4b04f9053176", "Alien Ant Farm");
            EnsureArtist("cc197bad-dc9c-440d-a5b5-d52ba2e14234", "Coldplay");

            var artists = Artist.All();

            artists.Should().NotBeNullOrEmpty();
            artists.Should().Contain(v => v.ForeignArtistId == "8ac6cc32-8ddf-43b1-9ac4-4b04f9053176");
            artists.Should().Contain(v => v.ForeignArtistId == "cc197bad-dc9c-440d-a5b5-d52ba2e14234");
        }

        [Test, Order(2)]
        public void get_artist_by_id()
        {
            var artist = EnsureArtist("8ac6cc32-8ddf-43b1-9ac4-4b04f9053176", "Alien Ant Farm");

            var result = Artist.Get(artist.Id);

            result.ForeignArtistId.Should().Be("8ac6cc32-8ddf-43b1-9ac4-4b04f9053176");
        }

        [Test]
        public void get_artist_by_unknown_id_should_return_404()
        {
            var result = Artist.InvalidGet(1000000);
        }

        [Test, Order(2)]
        public void update_artist_profile_id()
        {
            var artist = EnsureArtist("8ac6cc32-8ddf-43b1-9ac4-4b04f9053176", "Alien Ant Farm");

            var profileId = 1;
            if (artist.QualityProfileId == profileId)
            {
                profileId = 2;
            }

            artist.QualityProfileId = profileId;

            var result = Artist.Put(artist);

            Artist.Get(artist.Id).QualityProfileId.Should().Be(profileId);
        }

        [Test, Order(3)]
        public void update_artist_monitored()
        {
            var artist = EnsureArtist("f59c5520-5f46-4d2c-b2c4-822eabf53419", "Linkin Park", false);

            artist.Monitored.Should().BeFalse();
            //artist.Seasons.First().Monitored.Should().BeFalse();

            artist.Monitored = true;
            //artist.Seasons.ForEach(season =>
            //{
            //    season.Monitored = true;
            //});

            var result = Artist.Put(artist);

            result.Monitored.Should().BeTrue();
            //result.Seasons.First().Monitored.Should().BeTrue();
        }

        [Test, Order(3)]
        public void update_artist_tags()
        {
            var artist = EnsureArtist("8ac6cc32-8ddf-43b1-9ac4-4b04f9053176", "Alien Ant Farm");
            var tag = EnsureTag("abc");

            if (artist.Tags.Contains(tag.Id))
            {
                artist.Tags.Remove(tag.Id);

                var result = Artist.Put(artist);
                Artist.Get(artist.Id).Tags.Should().NotContain(tag.Id);
            }
            else
            {
                artist.Tags.Add(tag.Id);

                var result = Artist.Put(artist);
                Artist.Get(artist.Id).Tags.Should().Contain(tag.Id);
            }
        }

        [Test, Order(4)]
        public void delete_artist()
        {
            var artist = EnsureArtist("8ac6cc32-8ddf-43b1-9ac4-4b04f9053176", "Alien Ant Farm");

            Artist.Get(artist.Id).Should().NotBeNull();

            Artist.Delete(artist.Id);

            Artist.All().Should().NotContain(v => v.ForeignArtistId == "8ac6cc32-8ddf-43b1-9ac4-4b04f9053176");
        }
    }
}
