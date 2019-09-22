using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;
using NzbDrone.Core.Profiles.Metadata;
using NzbDrone.Common.Extensions;
using System;
using System.Data.SQLite;

namespace NzbDrone.Core.Test.MusicTests.ArtistRepositoryTests
{
    [TestFixture]

    public class ArtistRepositoryFixture : DbTest<ArtistRepository, Artist>
    {
        private ArtistRepository _artistRepo;
        private ArtistMetadataRepository _artistMetadataRepo;

        [SetUp]
        public void Setup()
        {
            _artistRepo = Mocker.Resolve<ArtistRepository>();
            _artistMetadataRepo = Mocker.Resolve<ArtistMetadataRepository>();
        }

        private void AddArtist(string name, string foreignId, List<string> oldIds = null)
        {
            if (oldIds == null)
            {
                oldIds = new List<string>();
            }

            var metadata = Builder<ArtistMetadata>.CreateNew()
                .With(a => a.Id = 0)
                .With(a => a.Name = name)
                .With(a=> a.OldForeignArtistIds = oldIds)
                .BuildNew();
            
            var artist = Builder<Artist>.CreateNew()
                .With(a => a.Id = 0)
                .With(a => a.Metadata = metadata)
                .With(a => a.CleanName = Parser.Parser.CleanArtistName(name))
                .With(a => a.ForeignArtistId = foreignId)
                .BuildNew();

            _artistMetadataRepo.Insert(metadata);
            artist.ArtistMetadataId = metadata.Id;
            _artistRepo.Insert(artist);
        }

        private void GivenArtists()
        {
            AddArtist("The Black Eyed Peas", "d5be5333-4171-427e-8e12-732087c6b78e");
            AddArtist("The Black Keys", "d15721d8-56b4-453d-b506-fc915b14cba2", new List<string> { "6f2ed437-825c-4cea-bb58-bf7688c6317a" });
        }

        [Test]
        public void should_lazyload_profiles()
        {
            var profile = new QualityProfile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities(Quality.FLAC, Quality.MP3_192, Quality.MP3_320),

                Cutoff = Quality.FLAC.Id,
                Name = "TestProfile"
            };

            var metaProfile = new MetadataProfile
            {
                Name = "TestProfile",
                PrimaryAlbumTypes = new List<ProfilePrimaryAlbumTypeItem>(),
                SecondaryAlbumTypes = new List<ProfileSecondaryAlbumTypeItem>(),
                ReleaseStatuses = new List<ProfileReleaseStatusItem>()
            };


            Mocker.Resolve<QualityProfileRepository>().Insert(profile);
            Mocker.Resolve<MetadataProfileRepository>().Insert(metaProfile);

            var artist = Builder<Artist>.CreateNew().BuildNew();
            artist.QualityProfileId = profile.Id;
            artist.MetadataProfileId = metaProfile.Id;

            Subject.Insert(artist);


            StoredModel.QualityProfile.Should().NotBeNull();
            StoredModel.MetadataProfile.Should().NotBeNull();

        }

        [TestCase("The Black Eyed Peas")]
        [TestCase("The Black Keys")]
        public void should_find_artist_in_db_by_name(string name)
        {
            GivenArtists();
            var artist = _artistRepo.FindByName(Parser.Parser.CleanArtistName(name));

            artist.Should().NotBeNull();
            artist.Name.Should().Be(name);
        }

        [Test]
        public void should_find_artist_in_by_id()
        {
            GivenArtists();
            var artist = _artistRepo.FindById("d5be5333-4171-427e-8e12-732087c6b78e");

            artist.Should().NotBeNull();
            artist.ForeignArtistId.Should().Be("d5be5333-4171-427e-8e12-732087c6b78e");
        }

        [Test]
        public void should_find_artist_in_by_old_id()
        {
            GivenArtists();
            var artist = _artistRepo.FindById("6f2ed437-825c-4cea-bb58-bf7688c6317a");

            artist.Should().NotBeNull();
            artist.Name.Should().Be("The Black Keys");
            artist.ForeignArtistId.Should().Be("d15721d8-56b4-453d-b506-fc915b14cba2");
        }

        [Test]
        public void should_not_find_artist_if_multiple_artists_have_same_name()
        {
            GivenArtists();

            string name = "Alice Cooper";
            AddArtist(name, "ee58c59f-8e7f-4430-b8ca-236c4d3745ae");
            AddArtist(name, "4d7928cd-7ed2-4282-8c29-c0c9f966f1bd");

            _artistRepo.All().Should().HaveCount(4);
            
            var artist = _artistRepo.FindByName(Parser.Parser.CleanArtistName(name));
            artist.Should().BeNull();
        }
        
        [Test]
        public void should_throw_sql_exception_adding_duplicate_artist()
        {
            var name = "test";
            var metadata = Builder<ArtistMetadata>.CreateNew()
                .With(a => a.Id = 0)
                .With(a => a.Name = name)
                .BuildNew();
            
            var artist1 = Builder<Artist>.CreateNew()
                .With(a => a.Id = 0)
                .With(a => a.Metadata = metadata)
                .With(a => a.CleanName = Parser.Parser.CleanArtistName(name))
                .BuildNew();

            var artist2 = artist1.JsonClone();
            artist2.Metadata = metadata;

            _artistMetadataRepo.Insert(metadata);
            _artistRepo.Insert(artist1);

            Action insertDupe = () => _artistRepo.Insert(artist2);
            insertDupe.Should().Throw<SQLiteException>();
        }
    }
}
