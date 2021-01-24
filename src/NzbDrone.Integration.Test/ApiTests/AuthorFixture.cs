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
        public void add_author_with_tags_should_store_them()
        {
            EnsureNoAuthor("14586394", "Andrew Hunter Murray");
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
        public void add_author_without_profileid_should_return_badrequest()
        {
            IgnoreOnMonoVersions("5.12", "5.14");

            EnsureNoAuthor("14586394", "Andrew Hunter Murray");

            var author = Author.Lookup("readarr:43765115").Single();

            author.Path = Path.Combine(AuthorRootFolder, author.AuthorName);

            Author.InvalidPost(author);
        }

        [Test]
        [Order(0)]
        public void add_author_without_path_should_return_badrequest()
        {
            IgnoreOnMonoVersions("5.12", "5.14");

            EnsureNoAuthor("14586394", "Andrew Hunter Murray");

            var author = Author.Lookup("readarr:43765115").Single();

            author.QualityProfileId = 1;

            Author.InvalidPost(author);
        }

        [Test]
        [Order(1)]
        public void add_author()
        {
            EnsureNoAuthor("14586394", "Andrew Hunter Murray");

            var author = Author.Lookup("readarr:43765115").Single();

            author.QualityProfileId = 1;
            author.MetadataProfileId = 1;
            author.Path = Path.Combine(AuthorRootFolder, author.AuthorName);

            var result = Author.Post(author);

            result.Should().NotBeNull();
            result.Id.Should().NotBe(0);
            result.QualityProfileId.Should().Be(1);
            result.MetadataProfileId.Should().Be(1);
            result.Path.Should().Be(Path.Combine(AuthorRootFolder, author.AuthorName));
        }

        [Test]
        [Order(2)]
        public void get_all_author()
        {
            EnsureAuthor("14586394", "43765115", "Andrew Hunter Murray");
            EnsureAuthor("383606", "16160797", "Robert Galbraith");

            var authors = Author.All();

            authors.Should().NotBeNullOrEmpty();
            authors.Should().Contain(v => v.ForeignAuthorId == "14586394");
            authors.Should().Contain(v => v.ForeignAuthorId == "383606");
        }

        [Test]
        [Order(2)]
        public void get_author_by_id()
        {
            var author = EnsureAuthor("14586394", "43765115", "Andrew Hunter Murray");

            var result = Author.Get(author.Id);

            result.ForeignAuthorId.Should().Be("14586394");
        }

        [Test]
        public void get_author_by_unknown_id_should_return_404()
        {
            IgnoreOnMonoVersions("5.12", "5.14");

            var result = Author.InvalidGet(1000000);
        }

        [Test]
        [Order(2)]
        public void update_author_profile_id()
        {
            var author = EnsureAuthor("14586394", "43765115", "Andrew Hunter Murray");

            var profileId = 1;
            if (author.QualityProfileId == profileId)
            {
                profileId = 2;
            }

            author.QualityProfileId = profileId;

            var result = Author.Put(author);

            Author.Get(author.Id).QualityProfileId.Should().Be(profileId);
        }

        [Test]
        [Order(3)]
        public void update_author_monitored()
        {
            var author = EnsureAuthor("14586394", "43765115", "Andrew Hunter Murray", false);

            author.Monitored.Should().BeFalse();

            author.Monitored = true;

            var result = Author.Put(author);

            result.Monitored.Should().BeTrue();
        }

        [Test]
        [Order(3)]
        public void update_author_tags()
        {
            var author = EnsureAuthor("14586394", "43765115", "Andrew Hunter Murray");
            var tag = EnsureTag("abc");

            if (author.Tags.Contains(tag.Id))
            {
                author.Tags.Remove(tag.Id);

                var result = Author.Put(author);
                Author.Get(author.Id).Tags.Should().NotContain(tag.Id);
            }
            else
            {
                author.Tags.Add(tag.Id);

                var result = Author.Put(author);
                Author.Get(author.Id).Tags.Should().Contain(tag.Id);
            }
        }

        [Test]
        [Order(4)]
        public void delete_author()
        {
            var author = EnsureAuthor("14586394", "43765115", "Andrew Hunter Murray");

            Author.Get(author.Id).Should().NotBeNull();

            Author.Delete(author.Id);

            Author.All().Should().NotContain(v => v.ForeignAuthorId == "14586394");
        }
    }
}
