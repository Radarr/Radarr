using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.ImportLists.HeadphonesImport;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;
using NzbDrone.Core.Profiles.Metadata;

namespace NzbDrone.Core.Test.Profiles.Metadata
{
    [TestFixture]

    public class MetadataProfileServiceFixture : CoreTest<MetadataProfileService>
    {
        [Test]
        public void init_should_add_default_profiles()
        {
            Subject.Handle(new ApplicationStartedEvent());

            Mocker.GetMock<IMetadataProfileRepository>()
                  .Verify(v => v.Insert(It.IsAny<MetadataProfile>()), Times.Once());
        }

        [Test]
        //This confirms that new profiles are added only if no other profiles exists.
        //We don't want to keep adding them back if a user deleted them on purpose.
        public void Init_should_skip_if_any_profiles_already_exist()
        {
            Mocker.GetMock<IMetadataProfileRepository>()
                  .Setup(s => s.All())
                  .Returns(Builder<MetadataProfile>.CreateListOfSize(2).Build().ToList());

            Subject.Handle(new ApplicationStartedEvent());

            Mocker.GetMock<IMetadataProfileRepository>()
                  .Verify(v => v.Insert(It.IsAny<MetadataProfile>()), Times.Never());
        }


        [Test]
        public void should_not_be_able_to_delete_profile_if_assigned_to_artist()
        {
            var profile = Builder<MetadataProfile>.CreateNew()
                .With(p => p.Id = 2)
                .Build();

            var artistList = Builder<Artist>.CreateListOfSize(3)
                                            .Random(1)
                                            .With(c => c.MetadataProfileId = profile.Id)
                                            .Build().ToList();

            var importLists = Builder<ImportListDefinition>.CreateListOfSize(2)
                .All()
                .With(c => c.MetadataProfileId = 1)
                .Build().ToList();

            Mocker.GetMock<IArtistService>().Setup(c => c.GetAllArtists()).Returns(artistList);
            Mocker.GetMock<IImportListFactory>().Setup(c => c.All()).Returns(importLists);
            Mocker.GetMock<IMetadataProfileRepository>().Setup(c => c.Get(profile.Id)).Returns(profile);

            Assert.Throws<MetadataProfileInUseException>(() => Subject.Delete(profile.Id));

            Mocker.GetMock<IMetadataProfileRepository>().Verify(c => c.Delete(It.IsAny<int>()), Times.Never());

        }

        [Test]
        public void should_not_be_able_to_delete_profile_if_assigned_to_import_list()
        {
            var profile = Builder<MetadataProfile>.CreateNew()
                .With(p => p.Id = 2)
                .Build();

            var artistList = Builder<Artist>.CreateListOfSize(3)
                .All()
                .With(c => c.MetadataProfileId = 1)
                .Build().ToList();

            var importLists = Builder<ImportListDefinition>.CreateListOfSize(2)
                .Random(1)
                .With(c => c.MetadataProfileId = profile.Id)
                .Build().ToList();

            Mocker.GetMock<IArtistService>().Setup(c => c.GetAllArtists()).Returns(artistList);
            Mocker.GetMock<IImportListFactory>().Setup(c => c.All()).Returns(importLists);
            Mocker.GetMock<IMetadataProfileRepository>().Setup(c => c.Get(profile.Id)).Returns(profile);

            Assert.Throws<MetadataProfileInUseException>(() => Subject.Delete(profile.Id));

            Mocker.GetMock<IMetadataProfileRepository>().Verify(c => c.Delete(It.IsAny<int>()), Times.Never());

        }


        [Test]
        public void should_delete_profile_if_not_assigned_to_artist_or_import_list()
        {
            var artistList = Builder<Artist>.CreateListOfSize(3)
                                            .All()
                                            .With(c => c.MetadataProfileId = 2)
                                            .Build().ToList();

            var importLists = Builder<ImportListDefinition>.CreateListOfSize(2)
                .All()
                .With(c => c.MetadataProfileId = 2)
                .Build().ToList();

            Mocker.GetMock<IArtistService>().Setup(c => c.GetAllArtists()).Returns(artistList);
            Mocker.GetMock<IImportListFactory>().Setup(c => c.All()).Returns(importLists);

            Subject.Delete(1);

            Mocker.GetMock<IMetadataProfileRepository>().Verify(c => c.Delete(1), Times.Once());
        }
    }
}
