using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MusicTests.AuthorRepositoryTests
{
    [TestFixture]

    public class AuthorMetadataRepositoryFixture : DbTest<AuthorMetadataRepository, AuthorMetadata>
    {
        private AuthorMetadataRepository _authorMetadataRepo;
        private List<AuthorMetadata> _metadataList;

        [SetUp]
        public void Setup()
        {
            _authorMetadataRepo = Mocker.Resolve<AuthorMetadataRepository>();
            _metadataList = Builder<AuthorMetadata>.CreateListOfSize(10).All().With(x => x.Id = 0).BuildList();
        }

        [Test]
        public void upsert_many_should_insert_list_of_new()
        {
            var updated = _authorMetadataRepo.UpsertMany(_metadataList);
            AllStoredModels.Should().HaveCount(_metadataList.Count);
            updated.Should().BeTrue();
        }

        [Test]
        public void upsert_many_should_upsert_existing_with_id_0()
        {
            var clone = _metadataList.JsonClone();
            var updated = _authorMetadataRepo.UpsertMany(clone);

            updated.Should().BeTrue();
            AllStoredModels.Should().HaveCount(_metadataList.Count);

            updated = _authorMetadataRepo.UpsertMany(_metadataList);
            updated.Should().BeFalse();
            AllStoredModels.Should().HaveCount(_metadataList.Count);
        }

        [Test]
        public void upsert_many_should_upsert_mixed_list_of_old_and_new()
        {
            var clone = _metadataList.Take(5).ToList().JsonClone();
            var updated = _authorMetadataRepo.UpsertMany(clone);

            updated.Should().BeTrue();
            AllStoredModels.Should().HaveCount(clone.Count);

            updated = _authorMetadataRepo.UpsertMany(_metadataList);
            updated.Should().BeTrue();
            AllStoredModels.Should().HaveCount(_metadataList.Count);
        }
    }
}
