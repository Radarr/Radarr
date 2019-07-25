using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;
using NzbDrone.Common.Extensions;
using System.Linq;

namespace NzbDrone.Core.Test.MusicTests.ArtistRepositoryTests
{
    [TestFixture]

    public class ArtistMetadataRepositoryFixture : DbTest<ArtistMetadataRepository, ArtistMetadata>
    {
        private ArtistMetadataRepository _artistMetadataRepo;
        private List<ArtistMetadata> _metadataList;

        [SetUp]
        public void Setup()
        {
            _artistMetadataRepo = Mocker.Resolve<ArtistMetadataRepository>();
            _metadataList = Builder<ArtistMetadata>.CreateListOfSize(10).BuildList();
        }
                 
        [Test]
        public void upsert_many_should_insert_list_of_new()
        {
            var updated = _artistMetadataRepo.UpsertMany(_metadataList);
            AllStoredModels.Should().HaveCount(_metadataList.Count);
            updated.Should().BeTrue();
        }
                 
        [Test]
        public void upsert_many_should_upsert_existing_with_id_0()
        {
            var _clone = _metadataList.JsonClone();
            var updated = _artistMetadataRepo.UpsertMany(_clone);

            updated.Should().BeTrue();
            AllStoredModels.Should().HaveCount(_metadataList.Count);

            updated = _artistMetadataRepo.UpsertMany(_metadataList);
            updated.Should().BeFalse();
            AllStoredModels.Should().HaveCount(_metadataList.Count);
        }

        [Test]
        public void upsert_many_should_upsert_mixed_list_of_old_and_new()
        {
            var _clone = _metadataList.Take(5).ToList().JsonClone();
            var updated = _artistMetadataRepo.UpsertMany(_clone);

            updated.Should().BeTrue();
            AllStoredModels.Should().HaveCount(_clone.Count);

            updated = _artistMetadataRepo.UpsertMany(_metadataList);
            updated.Should().BeTrue();
            AllStoredModels.Should().HaveCount(_metadataList.Count);
        }
    }
}
