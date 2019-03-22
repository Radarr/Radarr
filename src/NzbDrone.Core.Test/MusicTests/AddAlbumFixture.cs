using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Music;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MusicTests
{
    [TestFixture]
    public class AddAlbumFixture : CoreTest<AddAlbumService>
    {
        private Album _fakeAlbum;
        private AlbumRelease _fakeRelease;
        private List<ArtistMetadata> _fakeArtists;
        private readonly string _fakeArtistForeignId = "xxx-xxx-xxx";

        [SetUp]
        public void Setup()
        {
            _fakeAlbum = Builder<Album>
                .CreateNew()
                .Build();
            _fakeRelease = Builder<AlbumRelease>
                .CreateNew()
                .Build();
            _fakeRelease.Tracks = new List<Track>();
            _fakeAlbum.AlbumReleases = new List<AlbumRelease> {_fakeRelease};
        
            _fakeArtists = Builder<ArtistMetadata>.CreateListOfSize(1)
                .TheFirst(1)
                .With(x => x.ForeignArtistId = _fakeArtistForeignId)
                .Build() as List<ArtistMetadata>;
            
            Mocker.GetMock<IReleaseService>()
                .Setup(x => x.GetReleasesForRefresh(It.IsAny<int>(), It.IsAny<IEnumerable<string>>()))
                .Returns(new List<AlbumRelease>());

            Mocker.GetMock<IArtistMetadataRepository>()
                .Setup(x => x.FindById(_fakeArtistForeignId))
                .Returns(_fakeArtists[0]);
        }

        private void GivenValidAlbum(string lidarrId)
        {
            Mocker.GetMock<IProvideAlbumInfo>()
                .Setup(s => s.GetAlbumInfo(lidarrId))
                .Returns(new Tuple<string, Album, List<ArtistMetadata>>(_fakeArtistForeignId, _fakeAlbum, _fakeArtists));
        }

        [Test]
        public void should_be_able_to_add_an_album_without_passing_in_title()
        {
            var newAlbum = new Album
            {
                ForeignAlbumId = "ce09ea31-3d4a-4487-a797-e315175457a0"
            };

            GivenValidAlbum(newAlbum.ForeignAlbumId);

            var album = Subject.AddAlbum(newAlbum);

            album.Title.Should().Be(_fakeAlbum.Title);
        }

        [Test]
        public void should_throw_if_album_cannot_be_found()
        {
            var newAlbum = new Album
            {
                ForeignAlbumId = "ce09ea31-3d4a-4487-a797-e315175457a0"
            };

            Mocker.GetMock<IProvideAlbumInfo>()
                .Setup(s => s.GetAlbumInfo(newAlbum.ForeignAlbumId))
                .Throws(new AlbumNotFoundException(newAlbum.ForeignAlbumId));

            Assert.Throws<ValidationException>(() => Subject.AddAlbum(newAlbum));

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_not_add_duplicate_releases()
        {
            var newAlbum = Builder<Album>.CreateNew().Build();
            var releases = Builder<AlbumRelease>.CreateListOfSize(10)
                .All()
                .With(x => x.AlbumId = newAlbum.Id)
                .TheFirst(4)
                .With(x => x.ForeignReleaseId = "DuplicateId1")
                .TheLast(1)
                .With(x => x.ForeignReleaseId = "DuplicateId2")
                .Build() as List<AlbumRelease>;

            newAlbum.AlbumReleases = releases;
            
            var existingReleases = Builder<AlbumRelease>.CreateListOfSize(1)
                .All()
                .With(x => x.ForeignReleaseId = "DuplicateId2")
                .Build() as List<AlbumRelease>;
            
            Mocker.GetMock<IReleaseService>()
                .Setup(x => x.GetReleasesForRefresh(newAlbum.Id, It.IsAny<IEnumerable<string>>()))
                .Returns(existingReleases);
            
            var updatedReleases = Subject.AddAlbumReleases(newAlbum);
            
            updatedReleases.Should().HaveCount(7);
            
            Mocker.GetMock<IReleaseService>()
                .Verify(x => x.UpdateMany(It.Is<List<AlbumRelease>>(l => l.Count == 1 && l.Select(r => r.ForeignReleaseId).Distinct().Count() == 1)), Times.Once());
            
            Mocker.GetMock<IReleaseService>()
                .Verify(x => x.InsertMany(It.Is<List<AlbumRelease>>(l => l.Count == 6 && 
                                                                    l.Select(r => r.ForeignReleaseId).Distinct().Count() == 6 &&
                                                                    !l.Select(r => r.ForeignReleaseId).Contains("DuplicateId2"))), 
                        Times.Once());
        }
        
        [Test]
        public void should_only_add_one_monitored_release_ignoring_skyhook()
        {
            var newAlbum = Builder<Album>.CreateNew().Build();
            var releases = Builder<AlbumRelease>.CreateListOfSize(10)
                .All()
                .With(x => x.Monitored = true)
                .Build() as List<AlbumRelease>;

            newAlbum.AlbumReleases = releases;
            
            var updatedReleases = Subject.AddAlbumReleases(newAlbum);
            
            updatedReleases.Count(x => x.Monitored).Should().Be(1);
        }
        
        [Test]
        public void should_only_add_one_monitored_release_combining_with_existing()
        {
            var newAlbum = Builder<Album>.CreateNew().Build();
            
            var releases = Builder<AlbumRelease>.CreateListOfSize(10)
                .All()
                .With(x => x.Monitored = false)
                .Build() as List<AlbumRelease>;
            releases[1].Monitored = true;

            newAlbum.AlbumReleases = releases;
            
            var existingReleases = Builder<AlbumRelease>.CreateListOfSize(1)
                .All()
                .With(x => x.ForeignReleaseId = releases[0].ForeignReleaseId)
                .With(x => x.Monitored = true)
                .Build() as List<AlbumRelease>;
            
            Mocker.GetMock<IReleaseService>()
                .Setup(x => x.GetReleasesForRefresh(newAlbum.Id, It.IsAny<IEnumerable<string>>()))
                .Returns(existingReleases);
            
            var updatedReleases = Subject.AddAlbumReleases(newAlbum);
            
            updatedReleases.Count(x => x.Monitored).Should().Be(1);
        }
    }
}
