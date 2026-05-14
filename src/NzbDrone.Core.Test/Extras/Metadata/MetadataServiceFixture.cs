using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Extras.Metadata;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Extras.Metadata
{
    [TestFixture]
    public class MetadataServiceFixture : CoreTest<MetadataService>
    {
        private Movie _movie;
        private Mock<IMetadata> _consumer;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                                     .With(s => s.Path = @"C:\Test\Movies\The.Movie".AsOsAgnostic())
                                     .Build();

            _consumer = new Mock<IMetadata>();
            _consumer.SetupGet(c => c.SupportsMetadataWithoutVideoFile).Returns(true);
            _consumer.Setup(v => v.MovieImages(It.IsAny<Movie>()))
                     .Returns(new List<ImageFileResult>());

            Mocker.GetMock<IMetadataFactory>()
                  .Setup(v => v.Enabled())
                  .Returns(new List<IMetadata> { _consumer.Object });

            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.FolderExists(_movie.Path))
                  .Returns(true);

            Mocker.GetMock<IMetadataFileService>()
                  .Setup(v => v.GetFilesByMovie(_movie.Id))
                  .Returns(new List<MetadataFile>());
        }

        [Test]
        public void CreateAfterMovieScan_should_call_MovieMetadata_with_null_movieFile_if_no_files_and_supported()
        {
            Subject.CreateAfterMovieScan(_movie, new List<MovieFile>());

            _consumer.Verify(v => v.MovieMetadata(_movie, null), Times.Once());
        }

        [Test]
        public void CreateAfterMovieScan_should_not_call_MovieMetadata_with_null_movieFile_if_no_files_and_not_supported()
        {
            _consumer.SetupGet(c => c.SupportsMetadataWithoutVideoFile).Returns(false);

            Subject.CreateAfterMovieScan(_movie, new List<MovieFile>());

            _consumer.Verify(v => v.MovieMetadata(_movie, null), Times.Never());
        }

        [Test]
        public void CreateAfterMovieFolder_should_call_MovieMetadata_with_null_movieFile_if_supported()
        {
            Subject.CreateAfterMovieFolder(_movie, _movie.Path);

            _consumer.Verify(v => v.MovieMetadata(_movie, null), Times.Once());
        }

        [Test]
        public void CreateAfterMovieFolder_should_not_call_MovieMetadata_with_null_movieFile_if_not_supported()
        {
            _consumer.SetupGet(c => c.SupportsMetadataWithoutVideoFile).Returns(false);

            Subject.CreateAfterMovieFolder(_movie, _movie.Path);

            _consumer.Verify(v => v.MovieMetadata(_movie, null), Times.Never());
        }

        [Test]
        public void CreateAfterMovieScan_should_update_existing_metadata_with_MovieFileId_0_if_path_matches()
        {
            var movieFile = Builder<MovieFile>.CreateNew().With(f => f.Id = 123).Build();
            var existingMetadata = new MetadataFile
            {
                Id = 456,
                MovieId = _movie.Id,
                MovieFileId = 0,
                RelativePath = "movie.nfo",
                Type = MetadataType.MovieMetadata,
                Consumer = _consumer.Object.GetType().Name
            };

            Mocker.GetMock<IMetadataFileService>()
                  .Setup(v => v.GetFilesByMovie(_movie.Id))
                  .Returns(new List<MetadataFile> { existingMetadata });

            _consumer.Setup(c => c.MovieMetadata(_movie, movieFile))
                     .Returns(new MetadataFileResult("movie.nfo", "content"));

            Subject.CreateAfterMovieScan(_movie, new List<MovieFile> { movieFile });

            existingMetadata.MovieFileId.Should().Be(movieFile.Id);
            Mocker.GetMock<IMetadataFileService>()
                  .Verify(v => v.Upsert(It.Is<List<MetadataFile>>(l => l.Contains(existingMetadata))), Times.Once());
        }
    }
}
