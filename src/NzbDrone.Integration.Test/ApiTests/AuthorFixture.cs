using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class AuthorFixture : IntegrationTest
    {
        [Test]
        [Order(0)]
        public void add_artist_with_tags_should_store_them()
        {
            EnsureNoArtist("14586394", "Andrew Hunter Murray");
            var tag = EnsureTag("abc");

            var author = Author.Lookup("readarr:43765115").Single();

            author.QualityProfileId = 1;
            author.MetadataProfileId = 1;
            author.Path = Path.Combine(AuthorRootFolder, author.AuthorName);
            author.Tags = new HashSet<int>();
            author.Tags.Add(tag.Id);

            var result = Author.Post(author);

            result.Should().NotBeNull();
            result.Tags.Should().Equal(tag.Id);
        }

        [Test]
        [Order(0)]
        public void add_artist_without_profileid_should_return_badrequest()
        {
            IgnoreOnMonoVersions("5.12", "5.14");

            EnsureNoArtist("14586394", "Andrew Hunter Murray");

            var artist = Author.Lookup("readarr:43765115").Single();

            artist.Path = Path.Combine(AuthorRootFolder, artist.AuthorName);

            Author.InvalidPost(artist);
        }

        [Test]
        [Order(0)]
        public void add_artist_without_path_should_return_badrequest()
        {
            IgnoreOnMonoVersions("5.12", "5.14");

            EnsureNoArtist("14586394", "Andrew Hunter Murray");

            var artist = Author.Lookup("readarr:43765115").Single();

            artist.QualityProfileId = 1;

            Author.InvalidPost(artist);
        }

        [Test]
        [Order(1)]
        public void add_artist()
        {
            EnsureNoArtist("14586394", "Andrew Hunter Murray");

            var artist = Author.Lookup("readarr:43765115").Single();

            artist.QualityProfileId = 1;
            artist.MetadataProfileId = 1;
            artist.Path = Path.Combine(AuthorRootFolder, artist.AuthorName);

            var result = Author.Post(artist);

            result.Should().NotBeNull();
            result.Id.Should().NotBe(0);
            result.QualityProfileId.Should().Be(1);
            result.MetadataProfileId.Should().Be(1);
            result.Path.Should().Be(Path.Combine(AuthorRootFolder, artist.AuthorName));
        }

        [Test]
        [Order(2)]
        public void get_all_artist()
        {
            EnsureAuthor("14586394", "43765115", "Andrew Hunter Murray");
            EnsureAuthor("383606", "16160797", "Robert Galbraith");

            var artists = Author.All();

            artists.Should().NotBeNullOrEmpty();
            artists.Should().Contain(v => v.ForeignAuthorId == "14586394");
            artists.Should().Contain(v => v.ForeignAuthorId == "383606");
        }

        [Test]
        [Order(2)]
        public void get_artist_by_id()
        {
            var artist = EnsureAuthor("14586394", "43765115", "Andrew Hunter Murray");

            var result = Author.Get(artist.Id);

            result.ForeignAuthorId.Should().Be("14586394");
        }

        [Test]
        public void get_artist_by_unknown_id_should_return_404()
        {
            IgnoreOnMonoVersions("5.12", "5.14");

            var result = Author.InvalidGet(1000000);
        }

        [Test]
        [Order(2)]
        public void update_artist_profile_id()
        {
            var artist = EnsureAuthor("14586394", "43765115", "Andrew Hunter Murray");

            var profileId = 1;
            if (artist.QualityProfileId == profileId)
            {
                profileId = 2;
            }

            artist.QualityProfileId = profileId;

            var result = Author.Put(artist);

            Author.Get(artist.Id).QualityProfileId.Should().Be(profileId);
        }

        [Test]
        [Order(3)]
        public void update_artist_monitored()
        {
            var artist = EnsureAuthor("14586394", "43765115", "Andrew Hunter Murray", false);

            artist.Monitored.Should().BeFalse();

            artist.Monitored = true;

            var result = Author.Put(artist);

            result.Monitored.Should().BeTrue();
        }

        [Test]
        [Order(3)]
        public void update_artist_tags()
        {
            var artist = EnsureAuthor("14586394", "43765115", "Andrew Hunter Murray");
            var tag = EnsureTag("abc");

            if (artist.Tags.Contains(tag.Id))
            {
                artist.Tags.Remove(tag.Id);

                var result = Author.Put(artist);
                Author.Get(artist.Id).Tags.Should().NotContain(tag.Id);
            }
            else
            {
                artist.Tags.Add(tag.Id);

                var result = Author.Put(artist);
                Author.Get(artist.Id).Tags.Should().Contain(tag.Id);
            }
        }

        [Test]
        [Order(4)]
        public void delete_artist()
        {
            var artist = EnsureAuthor("14586394", "43765115", "Andrew Hunter Murray");

            Author.Get(artist.Id).Should().NotBeNull();

            Author.Delete(artist.Id);

            Author.All().Should().NotContain(v => v.ForeignAuthorId == "14586394");
        }
    }
}
