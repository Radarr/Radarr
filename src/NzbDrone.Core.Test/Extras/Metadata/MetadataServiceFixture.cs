using System.Collections.Generic;
using System.IO;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Extras.Metadata;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.MediaCover;
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
        private string _posterSource;
        private string _fanartSource;
        private List<MetadataFile> _metadataFiles;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                .With(m => m.Id = 1)
                .With(m => m.Path = @"C:\Test\Movies\Halloween (1978)".AsOsAgnostic())
                .Build();

            _posterSource = @"C:\Test\MediaCover\1\poster.jpg".AsOsAgnostic();
            _fanartSource = @"C:\Test\MediaCover\1\fanart.jpg".AsOsAgnostic();

            _consumer = new Mock<IMetadata>();
            _consumer.Setup(c => c.MovieImages(It.IsAny<Movie>()))
                     .Returns(new List<ImageFileResult>
                     {
                         new("poster.jpg", _posterSource),
                         new("fanart.jpg", _fanartSource)
                     });

            _metadataFiles = new List<MetadataFile>
            {
                new() { Id = 1, MovieId = 1, Consumer = _consumer.Object.GetType().Name, Type = MetadataType.MovieImage, RelativePath = "poster.jpg" },
                new() { Id = 2, MovieId = 1, Consumer = _consumer.Object.GetType().Name, Type = MetadataType.MovieImage, RelativePath = "fanart.jpg" }
            };

            Mocker.GetMock<IMetadataFactory>()
                  .Setup(c => c.Enabled())
                  .Returns(new List<IMetadata> { _consumer.Object });

            Mocker.GetMock<IMetadataFileService>()
                  .Setup(c => c.GetFilesByMovie(_movie.Id))
                  .Returns(() => _metadataFiles);

            Mocker.GetMock<IMapCoversToLocal>()
                  .Setup(c => c.GetCoverPath(_movie.Id, MediaCoverTypes.Poster, null))
                  .Returns(_posterSource);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(c => c.FolderExists(_movie.Path))
                  .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(c => c.FileExists(It.IsAny<string>()))
                  .Returns(true);
        }

        private void VerifyPosterCopied(Times times)
        {
            Mocker.GetMock<IDiskProvider>()
                  .Verify(c => c.CopyFile(_posterSource, Path.Combine(_movie.Path, "poster.jpg"), true), times);
        }

        [Test]
        public void should_overwrite_tracked_poster_when_poster_replacement_changed()
        {
            Subject.CreateAfterMediaCoverUpdate(_movie, true);

            VerifyPosterCopied(Times.Once());
        }

        [Test]
        public void should_not_overwrite_other_images_when_poster_replacement_changed()
        {
            Subject.CreateAfterMediaCoverUpdate(_movie, true);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(c => c.CopyFile(_fanartSource, It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void should_not_overwrite_existing_poster_on_regular_cover_update()
        {
            Subject.CreateAfterMediaCoverUpdate(_movie, false);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(c => c.CopyFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void should_not_overwrite_untracked_poster()
        {
            _metadataFiles.RemoveAll(f => f.RelativePath == "poster.jpg");

            Subject.CreateAfterMediaCoverUpdate(_movie, true);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(c => c.CopyFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void should_copy_missing_poster()
        {
            var destination = Path.Combine(_movie.Path, "poster.jpg");

            Mocker.GetMock<IDiskProvider>()
                  .Setup(c => c.FileExists(destination))
                  .Returns(false);

            Subject.CreateAfterMediaCoverUpdate(_movie, false);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(c => c.CopyFile(_posterSource, destination, false), Times.Once());
        }
    }
}
