using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Configuration.Events;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
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

            GivenPosterReplacementSettings(false);
        }

        private void GivenPosterReplacementSettings(bool enabled, string backgroundColor = "#1c1c1c")
        {
            Mocker.GetMock<IConfigService>().SetupGet(c => c.PosterReplacementEnabled).Returns(enabled);
            Mocker.GetMock<IConfigService>().SetupGet(c => c.PosterReplacementGenres).Returns(new List<string> { "Horror" });
            Mocker.GetMock<IConfigService>().SetupGet(c => c.PosterReplacementTags).Returns(new List<int>());
            Mocker.GetMock<IConfigService>().SetupGet(c => c.PosterReplacementBackgroundColor).Returns(backgroundColor);
            Mocker.GetMock<IConfigService>().SetupGet(c => c.PosterReplacementTextColor).Returns("#ffffff");
        }

        private PosterReplacement GivenPosterIsReplaced()
        {
            var replacement = new PosterReplacement("Halloween", "#1c1c1c", "#ffffff");

            Mocker.GetMock<IPosterReplacementService>()
                  .Setup(v => v.GetReplacement(It.IsAny<Movie>()))
                  .Returns(replacement);

            return replacement;
        }

        private void GivenReplacementOnDisk(string signature)
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.FileExists(It.IsAny<string>()))
                  .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.ReadAllText(It.Is<string>(p => p.EndsWith("poster.replacement"))))
                  .Returns(signature);
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

        [Test]
        public void should_render_replacement_poster_instead_of_downloading()
        {
            var replacement = GivenPosterIsReplaced();

            Subject.HandleAsync(new MovieUpdatedEvent(_movie));

            Mocker.GetMock<IPosterRenderer>()
                  .Verify(v => v.Render(replacement, It.Is<string>(p => p.EndsWith("poster.jpg"))), Times.Once());

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.WriteAllText(It.Is<string>(p => p.EndsWith("poster.replacement")), replacement.Signature), Times.Once());

            Mocker.GetMock<ICoverExistsSpecification>()
                  .Verify(v => v.AlreadyExists(It.IsAny<string>(), It.IsAny<string>()), Times.Never());

            Mocker.GetMock<IHttpClient>()
                  .Verify(v => v.DownloadFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never());

            Mocker.GetMock<IImageResizer>()
                  .Verify(v => v.Resize(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Exactly(2));

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.Is<MediaCoversUpdatedEvent>(e => e.Updated && e.PosterReplacementChanged)), Times.Once());
        }

        [Test]
        public void should_not_render_replacement_poster_again_if_current()
        {
            var replacement = GivenPosterIsReplaced();
            GivenReplacementOnDisk(replacement.Signature);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.GetFileSize(It.IsAny<string>()))
                  .Returns(1000);

            Subject.HandleAsync(new MovieUpdatedEvent(_movie));

            Mocker.GetMock<IPosterRenderer>()
                  .Verify(v => v.Render(It.IsAny<PosterReplacement>(), It.IsAny<string>()), Times.Never());

            Mocker.GetMock<IImageResizer>()
                  .Verify(v => v.Resize(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.Is<MediaCoversUpdatedEvent>(e => !e.Updated && !e.PosterReplacementChanged)), Times.Once());
        }

        [Test]
        public void should_render_replacement_poster_again_if_settings_changed()
        {
            GivenPosterIsReplaced();
            GivenReplacementOnDisk("outdated");

            Subject.HandleAsync(new MovieUpdatedEvent(_movie));

            Mocker.GetMock<IPosterRenderer>()
                  .Verify(v => v.Render(It.IsAny<PosterReplacement>(), It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_restore_original_poster_when_no_longer_replaced()
        {
            GivenReplacementOnDisk("previous");

            Mocker.GetMock<ICoverExistsSpecification>()
                  .Setup(v => v.AlreadyExists(It.IsAny<string>(), It.IsAny<string>()))
                  .Returns(false);

            Subject.HandleAsync(new MovieUpdatedEvent(_movie));

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.DeleteFile(It.Is<string>(p => p.EndsWith("poster.jpg"))), Times.Once());

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.DeleteFile(It.Is<string>(p => p.EndsWith("poster.replacement"))), Times.Once());

            Mocker.GetMock<IHttpClient>()
                  .Verify(v => v.DownloadFile(It.IsAny<string>(), It.Is<string>(p => p.EndsWith("poster.jpg"))), Times.Once());

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.Is<MediaCoversUpdatedEvent>(e => e.Updated && e.PosterReplacementChanged)), Times.Once());
        }

        [Test]
        public void should_change_poster_url_hash_when_poster_is_replaced()
        {
            var covers = new List<MediaCover.MediaCover>
            {
                new() { CoverType = MediaCoverTypes.Poster, RemoteUrl = "https://artworks.examples.com/posters/1.jpg" }
            };

            GivenReplacementOnDisk("signature");

            Subject.ConvertToLocalUrls(12, covers, DateTime.UtcNow.AddDays(-2));

            covers.Single().Url.Should().StartWith("/MediaCover/12/poster.jpg?h=");
            covers.Single().Url.Should().NotBe("/MediaCover/12/poster.jpg?h=2a57c239a7baaae159e7");
        }

        [Test]
        public void should_not_change_url_hash_of_other_covers_when_poster_is_replaced()
        {
            var covers = new List<MediaCover.MediaCover>
            {
                new() { CoverType = MediaCoverTypes.Banner, RemoteUrl = "https://artworks.examples.com/banners/1.jpg" }
            };

            GivenReplacementOnDisk("signature");

            Subject.ConvertToLocalUrls(12, covers, DateTime.UtcNow);

            covers.Single().Url.Should().Be("/MediaCover/12/banner.jpg?h=a6210a45e2b93963ad9e");
        }

        [Test]
        public void should_only_apply_poster_replacement_on_edit_when_outdated()
        {
            GivenPosterIsReplaced();

            Subject.HandleAsync(new MovieEditedEvent(_movie, _movie));
            Subject.HandleAsync(new MovieEditedEvent(_movie, _movie));

            Mocker.GetMock<IPosterRenderer>()
                  .Verify(v => v.Render(It.IsAny<PosterReplacement>(), It.IsAny<string>()), Times.Once());

            Mocker.GetMock<ICoverExistsSpecification>()
                  .Verify(v => v.AlreadyExists(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_queue_command_only_when_poster_replacement_settings_change()
        {
            Subject.Handle(new ApplicationStartedEvent());

            Subject.HandleAsync(new ConfigSavedEvent());

            Mocker.GetMock<IManageCommandQueue>()
                  .Verify(v => v.Push(It.IsAny<ApplyPosterReplacementCommand>(), It.IsAny<CommandPriority>(), It.IsAny<CommandTrigger>()), Times.Never());

            GivenPosterReplacementSettings(true, "#550000");
            Subject.HandleAsync(new ConfigSavedEvent());
            Subject.HandleAsync(new ConfigSavedEvent());

            Mocker.GetMock<IManageCommandQueue>()
                  .Verify(v => v.Push(It.IsAny<ApplyPosterReplacementCommand>(), It.IsAny<CommandPriority>(), It.IsAny<CommandTrigger>()), Times.Once());
        }

        [Test]
        public void should_only_process_outdated_movies_when_applying_poster_replacement()
        {
            var replacedMovie = Builder<Movie>.CreateNew()
                .With(v => v.Id = 3)
                .With(v => v.MovieMetadata.Value.Images = new List<MediaCover.MediaCover> { new(MediaCoverTypes.Poster, "") })
                .Build();

            Mocker.GetMock<IMovieService>()
                  .Setup(v => v.GetAllMovies())
                  .Returns(new List<Movie> { _movie, replacedMovie });

            Mocker.GetMock<IPosterReplacementService>()
                  .Setup(v => v.GetReplacement(It.Is<Movie>(m => m.Id == replacedMovie.Id)))
                  .Returns(new PosterReplacement("Halloween", "#1c1c1c", "#ffffff"));

            Subject.Execute(new ApplyPosterReplacementCommand());

            Mocker.GetMock<IPosterRenderer>()
                  .Verify(v => v.Render(It.IsAny<PosterReplacement>(), It.Is<string>(p => p.Contains(Path.Combine("MediaCover", "3")))), Times.Once());

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.IsAny<MediaCoversUpdatedEvent>()), Times.Once());
        }
    }
}
