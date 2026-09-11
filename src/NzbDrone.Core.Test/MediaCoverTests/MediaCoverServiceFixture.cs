using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Events;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MediaCoverTests
{
    [TestFixture]
    public class MediaCoverServiceFixture : CoreTest<MediaCoverService>
    {
        private Movie _movie;

        [SetUp]
        public void Setup()
        {
            Mocker.SetConstant<IAppFolderInfo>(new AppFolderInfo(Mocker.Resolve<IStartupContext>()));

            _movie = Builder<Movie>.CreateNew()
                .With(v => v.Id = 2)
                .With(v => v.Added = DateTime.UtcNow)
                .With(v => v.MovieMetadata.Value.Images = new List<MediaCover.MediaCover> { new(MediaCoverTypes.Poster, "") })
                .Build();
        }

        [Test]
        public void should_convert_cover_urls_to_local()
        {
            var covers = new List<MediaCover.MediaCover>
            {
                new() { CoverType = MediaCoverTypes.Banner, RemoteUrl = "https://artworks.examples.com/banners/1.jpg" }
            };

            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.FileExists(It.IsAny<string>()))
                  .Returns(true);

            Subject.ConvertToLocalUrls(12, covers, DateTime.UtcNow);

            covers.Single().Url.Should().Be("/MediaCover/12/banner.jpg?h=a6210a45e2b93963ad9e");
        }

        [Test]
        public void should_convert_cover_urls_to_local_without_hash_if_cover_has_not_been_downloaded()
        {
            var covers = new List<MediaCover.MediaCover>
            {
                new() { CoverType = MediaCoverTypes.Banner, RemoteUrl = "https://artworks.examples.com/banners/1.jpg" }
            };

            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.FileExists(It.IsAny<string>()))
                  .Returns(false);

            Subject.ConvertToLocalUrls(12, covers, DateTime.UtcNow);

            covers.Single().Url.Should().Be("/MediaCover/12/banner.jpg");
        }

        [Test]
        public void should_only_check_if_cover_exists_on_disk_once()
        {
            var covers = new List<MediaCover.MediaCover>
            {
                new() { CoverType = MediaCoverTypes.Banner, RemoteUrl = "https://artworks.examples.com/banners/1.jpg" }
            };

            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.FileExists(It.IsAny<string>()))
                  .Returns(true);

            Subject.ConvertToLocalUrls(12, covers, DateTime.UtcNow);
            Subject.ConvertToLocalUrls(12, covers, DateTime.UtcNow);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.FileExists(It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_add_hash_to_cover_url_once_cover_exists()
        {
            var covers = new List<MediaCover.MediaCover>
            {
                new() { CoverType = MediaCoverTypes.Poster, RemoteUrl = "https://artworks.examples.com/posters/1.jpg" }
            };

            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.FileExists(It.IsAny<string>()))
                  .Returns(false);

            Subject.ConvertToLocalUrls(_movie.Id, covers, _movie.Added);

            covers.Single().Url.Should().Be($"/MediaCover/{_movie.Id}/poster.jpg");

            Mocker.GetMock<ICoverExistsSpecification>()
                  .Setup(v => v.AlreadyExists(It.IsAny<string>(), It.IsAny<string>()))
                  .Returns(true);

            Subject.HandleAsync(new MovieUpdatedEvent(_movie));
            Subject.ConvertToLocalUrls(_movie.Id, covers, _movie.Added);

            covers.Single().Url.Should().Be($"/MediaCover/{_movie.Id}/poster.jpg?h=2a57c239a7baaae159e7");
        }

        [Test]
        public void should_convert_media_urls_to_local_without_hash_if_remote_url_is_empty()
        {
            var covers = new List<MediaCover.MediaCover>
                {
                    new() { CoverType = MediaCoverTypes.Banner }
                };

            Subject.ConvertToLocalUrls(12, covers, DateTime.UtcNow);

            covers.Single().Url.Should().Be("/MediaCover/12/banner.jpg");
        }

        [Test]
        public void should_not_check_if_cover_exists_for_movie_added_more_than_a_day_ago()
        {
            var covers = new List<MediaCover.MediaCover>
            {
                new() { CoverType = MediaCoverTypes.Banner, RemoteUrl = "https://artworks.examples.com/banners/1.jpg" }
            };

            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.FileExists(It.IsAny<string>()))
                  .Returns(false);

            Subject.ConvertToLocalUrls(12, covers, DateTime.UtcNow.AddDays(-2));

            covers.Single().Url.Should().Be("/MediaCover/12/banner.jpg?h=a6210a45e2b93963ad9e");

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.FileExists(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_resize_covers_if_main_downloaded()
        {
            Mocker.GetMock<ICoverExistsSpecification>()
                  .Setup(v => v.AlreadyExists(It.IsAny<string>(), It.IsAny<string>()))
                  .Returns(false);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.FileExists(It.IsAny<string>()))
                  .Returns(true);

            Subject.HandleAsync(new MovieUpdatedEvent(_movie));

            Mocker.GetMock<IImageResizer>()
                  .Verify(v => v.Resize(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Exactly(2));
        }

        [Test]
        public void should_resize_covers_if_missing()
        {
            Mocker.GetMock<ICoverExistsSpecification>()
                  .Setup(v => v.AlreadyExists(It.IsAny<string>(), It.IsAny<string>()))
                  .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.FileExists(It.IsAny<string>()))
                  .Returns(false);

            Subject.HandleAsync(new MovieUpdatedEvent(_movie));

            Mocker.GetMock<IImageResizer>()
                  .Verify(v => v.Resize(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Exactly(2));
        }

        [Test]
        public void should_not_resize_covers_if_exists()
        {
            Mocker.GetMock<ICoverExistsSpecification>()
                  .Setup(v => v.AlreadyExists(It.IsAny<string>(), It.IsAny<string>()))
                  .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.FileExists(It.IsAny<string>()))
                  .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.GetFileSize(It.IsAny<string>()))
                  .Returns(1000);

            Subject.HandleAsync(new MovieUpdatedEvent(_movie));

            Mocker.GetMock<IImageResizer>()
                  .Verify(v => v.Resize(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
        }

        [Test]
        public void should_resize_covers_if_existing_is_empty()
        {
            Mocker.GetMock<ICoverExistsSpecification>()
                  .Setup(v => v.AlreadyExists(It.IsAny<string>(), It.IsAny<string>()))
                  .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.FileExists(It.IsAny<string>()))
                  .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.GetFileSize(It.IsAny<string>()))
                  .Returns(0);

            Subject.HandleAsync(new MovieUpdatedEvent(_movie));

            Mocker.GetMock<IImageResizer>()
                  .Verify(v => v.Resize(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Exactly(2));
        }

        [Test]
        public void should_log_error_if_resize_failed()
        {
            Mocker.GetMock<ICoverExistsSpecification>()
                  .Setup(v => v.AlreadyExists(It.IsAny<string>(), It.IsAny<string>()))
                  .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.FileExists(It.IsAny<string>()))
                  .Returns(false);

            Mocker.GetMock<IImageResizer>()
                  .Setup(v => v.Resize(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                  .Throws<ApplicationException>();

            Subject.HandleAsync(new MovieUpdatedEvent(_movie));

            Mocker.GetMock<IImageResizer>()
                  .Verify(v => v.Resize(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Exactly(2));
        }
    }
}
