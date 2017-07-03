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
            EnsureNoArtsit("266189", "Alien Ant Farm");
            var tag = EnsureTag("abc");

            var artist = Artist.Lookup("266189").Single();

            artist.ProfileId = 1;
            artist.Path = Path.Combine(ArtistRootFolder, artist.Name);
            artist.Tags = new HashSet<int>();
            artist.Tags.Add(tag.Id);

            var result = Artist.Post(artist);

            result.Should().NotBeNull();
            result.Tags.Should().Equal(tag.Id);
        }

        [Test, Order(0)]
        public void add_artist_without_profileid_should_return_badrequest()
        {
            EnsureNoArtsit("266189", "Alien Ant Farm");

            var artist = Artist.Lookup("tvdb:266189").Single();

            artist.Path = Path.Combine(ArtistRootFolder, artist.Name);

            Artist.InvalidPost(artist);
        }

        [Test, Order(0)]
        public void add_artist_without_path_should_return_badrequest()
        {
            EnsureNoArtsit("266189", "Alien Ant Farm");

            var artist = Artist.Lookup("lidarr:266189").Single();

            artist.ProfileId = 1;

            Artist.InvalidPost(artist);
        }

        [Test, Order(1)]
        public void add_artist()
        {
            EnsureNoArtsit("266189", "Alien Ant Farm");

            var artist = Artist.Lookup("lidarr:266189").Single();

            artist.ProfileId = 1;
            artist.Path = Path.Combine(ArtistRootFolder, artist.Name);

            var result = Artist.Post(artist);

            result.Should().NotBeNull();
            result.Id.Should().NotBe(0);
            result.ProfileId.Should().Be(1);
            result.Path.Should().Be(Path.Combine(ArtistRootFolder, artist.Name));
        }


        [Test, Order(2)]
        public void get_all_artist()
        {
            EnsureArtist("266189", "Alien Ant Farm");
            EnsureArtist("73065", "Coldplay");

            Artist.All().Should().NotBeNullOrEmpty();
            Artist.All().Should().Contain(v => v.ForeignArtistId == "73065");
            Artist.All().Should().Contain(v => v.ForeignArtistId == "266189");
        }

        [Test, Order(2)]
        public void get_artist_by_id()
        {
            var artist = EnsureArtist("266189", "Alien Ant Farm");

            var result = Artist.Get(artist.Id);

            result.ForeignArtistId.Should().Be("266189");
        }

        [Test]
        public void get_artist_by_unknown_id_should_return_404()
        {
            var result = Artist.InvalidGet(1000000);
        }

        [Test, Order(2)]
        public void update_artist_profile_id()
        {
            var artist = EnsureArtist("266189", "Alien Ant Farm");

            var profileId = 1;
            if (artist.ProfileId == profileId)
            {
                profileId = 2;
            }

            artist.ProfileId = profileId;

            var result = Artist.Put(artist);

            Artist.Get(artist.Id).ProfileId.Should().Be(profileId);
        }

        [Test, Order(3)]
        public void update_artist_monitored()
        {
            var artist = EnsureArtist("266189", "Alien Ant Farm", false);

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
            var artist = EnsureArtist("266189", "Alien Ant Farm");
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
            var artist = EnsureArtist("266189", "Alien Ant Farm");

            Artist.Get(artist.Id).Should().NotBeNull();

            Artist.Delete(artist.Id);

            Artist.All().Should().NotContain(v => v.ForeignArtistId == "266189");
        }
    }
}