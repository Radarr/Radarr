using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class ArtistFixture : IntegrationTest
    {
        [Test]
        [Order(0)]
        public void add_artist_with_tags_should_store_them()
        {
            EnsureNoArtist("amzn1.gr.author.v1.SHA8asP5mFyLIP9NlujvLQ", "J.K. Rowling");
            var tag = EnsureTag("abc");

            var artist = Artist.Lookup("readarr:1").Single();

            artist.QualityProfileId = 1;
            artist.MetadataProfileId = 1;
            artist.Path = Path.Combine(ArtistRootFolder, artist.ArtistName);
            artist.Tags = new HashSet<int>();
            artist.Tags.Add(tag.Id);

            var result = Artist.Post(artist);

            result.Should().NotBeNull();
            result.Tags.Should().Equal(tag.Id);
        }

        [Test]
        [Order(0)]
        public void add_artist_without_profileid_should_return_badrequest()
        {
            IgnoreOnMonoVersions("5.12", "5.14");

            EnsureNoArtist("amzn1.gr.author.v1.SHA8asP5mFyLIP9NlujvLQ", "J.K. Rowling");

            var artist = Artist.Lookup("readarr:1").Single();

            artist.Path = Path.Combine(ArtistRootFolder, artist.ArtistName);

            Artist.InvalidPost(artist);
        }

        [Test]
        [Order(0)]
        public void add_artist_without_path_should_return_badrequest()
        {
            IgnoreOnMonoVersions("5.12", "5.14");

            EnsureNoArtist("amzn1.gr.author.v1.SHA8asP5mFyLIP9NlujvLQ", "J.K. Rowling");

            var artist = Artist.Lookup("readarr:1").Single();

            artist.QualityProfileId = 1;

            Artist.InvalidPost(artist);
        }

        [Test]
        [Order(1)]
        public void add_artist()
        {
            EnsureNoArtist("amzn1.gr.author.v1.SHA8asP5mFyLIP9NlujvLQ", "J.K. Rowling");

            var artist = Artist.Lookup("readarr:1").Single();

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

        [Test]
        [Order(2)]
        public void get_all_artist()
        {
            EnsureArtist("amzn1.gr.author.v1.SHA8asP5mFyLIP9NlujvLQ", "1", "J.K. Rowling");
            EnsureArtist("amzn1.gr.author.v1.qTrNu9-PIaaBj5gYRDmN4Q", "34497", "Terry Pratchett");

            var artists = Artist.All();

            artists.Should().NotBeNullOrEmpty();
            artists.Should().Contain(v => v.ForeignAuthorId == "amzn1.gr.author.v1.SHA8asP5mFyLIP9NlujvLQ");
            artists.Should().Contain(v => v.ForeignAuthorId == "amzn1.gr.author.v1.qTrNu9-PIaaBj5gYRDmN4Q");
        }

        [Test]
        [Order(2)]
        public void get_artist_by_id()
        {
            var artist = EnsureArtist("amzn1.gr.author.v1.SHA8asP5mFyLIP9NlujvLQ", "1", "J.K. Rowling");

            var result = Artist.Get(artist.Id);

            result.ForeignAuthorId.Should().Be("amzn1.gr.author.v1.SHA8asP5mFyLIP9NlujvLQ");
        }

        [Test]
        public void get_artist_by_unknown_id_should_return_404()
        {
            IgnoreOnMonoVersions("5.12", "5.14");

            var result = Artist.InvalidGet(1000000);
        }

        [Test]
        [Order(2)]
        public void update_artist_profile_id()
        {
            var artist = EnsureArtist("amzn1.gr.author.v1.SHA8asP5mFyLIP9NlujvLQ", "1", "J.K. Rowling");

            var profileId = 1;
            if (artist.QualityProfileId == profileId)
            {
                profileId = 2;
            }

            artist.QualityProfileId = profileId;

            var result = Artist.Put(artist);

            Artist.Get(artist.Id).QualityProfileId.Should().Be(profileId);
        }

        [Test]
        [Order(3)]
        public void update_artist_monitored()
        {
            var artist = EnsureArtist("amzn1.gr.author.v1.SHA8asP5mFyLIP9NlujvLQ", "1", "J.K. Rowling", false);

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

        [Test]
        [Order(3)]
        public void update_artist_tags()
        {
            var artist = EnsureArtist("amzn1.gr.author.v1.SHA8asP5mFyLIP9NlujvLQ", "1", "J.K. Rowling");
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

        [Test]
        [Order(4)]
        public void delete_artist()
        {
            var artist = EnsureArtist("amzn1.gr.author.v1.SHA8asP5mFyLIP9NlujvLQ", "1", "J.K. Rowling");

            Artist.Get(artist.Id).Should().NotBeNull();

            Artist.Delete(artist.Id);

            Artist.All().Should().NotContain(v => v.ForeignAuthorId == "amzn1.gr.author.v1.SHA8asP5mFyLIP9NlujvLQ");
        }
    }
}
