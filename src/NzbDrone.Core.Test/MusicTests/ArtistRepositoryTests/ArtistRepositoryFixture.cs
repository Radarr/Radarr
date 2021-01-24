using System;
using System.Collections.Generic;
using System.Data.SQLite;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books;
using NzbDrone.Core.Profiles.Metadata;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MusicTests.AuthorRepositoryTests
{
    [TestFixture]

    public class AuthorRepositoryFixture : DbTest<AuthorRepository, Author>
    {
        private AuthorRepository _authorRepo;
        private AuthorMetadataRepository _authorMetadataRepo;

        [SetUp]
        public void Setup()
        {
            _authorRepo = Mocker.Resolve<AuthorRepository>();
            _authorMetadataRepo = Mocker.Resolve<AuthorMetadataRepository>();
        }

        private void AddAuthor(string name, string foreignId, List<string> oldIds = null)
        {
            if (oldIds == null)
            {
                oldIds = new List<string>();
            }

            var metadata = Builder<AuthorMetadata>.CreateNew()
                .With(a => a.Id = 0)
                .With(a => a.Name = name)
                .With(a => a.TitleSlug = foreignId)
                .BuildNew();

            var author = Builder<Author>.CreateNew()
                .With(a => a.Id = 0)
                .With(a => a.Metadata = metadata)
                .With(a => a.CleanName = Parser.Parser.CleanAuthorName(name))
                .With(a => a.ForeignAuthorId = foreignId)
                .BuildNew();

            _authorMetadataRepo.Insert(metadata);
            author.AuthorMetadataId = metadata.Id;
            _authorRepo.Insert(author);
        }

        private void GivenAuthors()
        {
            AddAuthor("The Black Eyed Peas", "d5be5333-4171-427e-8e12-732087c6b78e");
            AddAuthor("The Black Keys", "d15721d8-56b4-453d-b506-fc915b14cba2", new List<string> { "6f2ed437-825c-4cea-bb58-bf7688c6317a" });
        }

        [Test]
        public void should_lazyload_profiles()
        {
            var profile = new QualityProfile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities(Quality.FLAC, Quality.MP3_320, Quality.MP3_320),

                Cutoff = Quality.FLAC.Id,
                Name = "TestProfile"
            };

            var metaProfile = new MetadataProfile
            {
                Name = "TestProfile"
            };

            Mocker.Resolve<QualityProfileRepository>().Insert(profile);
            Mocker.Resolve<MetadataProfileRepository>().Insert(metaProfile);

            var author = Builder<Author>.CreateNew().BuildNew();
            author.QualityProfileId = profile.Id;
            author.MetadataProfileId = metaProfile.Id;

            Subject.Insert(author);

            StoredModel.QualityProfile.Should().NotBeNull();
            StoredModel.MetadataProfile.Should().NotBeNull();
        }

        [TestCase("The Black Eyed Peas")]
        [TestCase("The Black Keys")]
        public void should_find_author_in_db_by_name(string name)
        {
            GivenAuthors();
            var author = _authorRepo.FindByName(Parser.Parser.CleanAuthorName(name));

            author.Should().NotBeNull();
            author.Name.Should().Be(name);
        }

        [Test]
        public void should_find_author_in_by_id()
        {
            GivenAuthors();
            var author = _authorRepo.FindById("d5be5333-4171-427e-8e12-732087c6b78e");

            author.Should().NotBeNull();
            author.ForeignAuthorId.Should().Be("d5be5333-4171-427e-8e12-732087c6b78e");
        }

        [Test]
        public void should_not_find_author_if_multiple_authors_have_same_name()
        {
            GivenAuthors();

            string name = "Alice Cooper";
            AddAuthor(name, "ee58c59f-8e7f-4430-b8ca-236c4d3745ae");
            AddAuthor(name, "4d7928cd-7ed2-4282-8c29-c0c9f966f1bd");

            _authorRepo.All().Should().HaveCount(4);

            var author = _authorRepo.FindByName(Parser.Parser.CleanAuthorName(name));
            author.Should().BeNull();
        }

        [Test]
        public void should_throw_sql_exception_adding_duplicate_author()
        {
            var name = "test";
            var metadata = Builder<AuthorMetadata>.CreateNew()
                .With(a => a.Id = 0)
                .With(a => a.Name = name)
                .BuildNew();

            var author1 = Builder<Author>.CreateNew()
                .With(a => a.Id = 0)
                .With(a => a.Metadata = metadata)
                .With(a => a.CleanName = Parser.Parser.CleanAuthorName(name))
                .BuildNew();

            var author2 = author1.JsonClone();
            author2.Metadata = metadata;

            _authorMetadataRepo.Insert(metadata);
            _authorRepo.Insert(author1);

            Action insertDupe = () => _authorRepo.Insert(author2);
            insertDupe.Should().Throw<SQLiteException>();
        }
    }
}
