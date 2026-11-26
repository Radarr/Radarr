using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class MountCheckFixture : CoreTest<MountCheck>
    {
        private readonly string _moviePath = @"C:\Movies\Movie (2025)".AsOsAgnostic();
        private readonly string _otherMoviePath = @"C:\Movies\Other Movie (2025)".AsOsAgnostic();
        private readonly string _readOnlyMoviePath = @"D:\Movies\Movie (2025)".AsOsAgnostic();
        private readonly string _otherReadOnlyMoviePath = @"D:\Movies\Other Movie (2025)".AsOsAgnostic();

        private IMount _writableMount;
        private IMount _readOnlyMount;

        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<ILocalizationService>()
                  .Setup(s => s.GetLocalizedString(It.IsAny<string>()))
                  .Returns("Mount containing a movie path is mounted read-only: ");

            _writableMount = CreateMount("/dev/sda1", @"C:\".AsOsAgnostic(), false);
            _readOnlyMount = CreateMount("/dev/sdb1", @"D:\".AsOsAgnostic(), true);

            GivenMovies();
        }

        private static IMount CreateMount(string name, string rootDirectory, bool? readOnly)
        {
            var options = readOnly.HasValue
                ? new MountOptions(new Dictionary<string, string> { { readOnly.Value ? "ro" : "rw", string.Empty } })
                : null;

            var mount = new Mock<IMount>();
            mount.SetupGet(s => s.Name).Returns(name);
            mount.SetupGet(s => s.RootDirectory).Returns(rootDirectory);
            mount.SetupGet(s => s.MountOptions).Returns(options);

            return mount.Object;
        }

        private void GivenMovies(params (string Path, IMount Mount)[] movies)
        {
            Mocker.GetMock<IMovieService>()
                  .Setup(s => s.AllMoviePaths())
                  .Returns(movies.Select((m, i) => (Id: i + 1, m.Path)).ToDictionary(m => m.Id, m => m.Path));

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetMountsForPaths(It.IsAny<HashSet<string>>()))
                  .Returns(movies.ToDictionary(m => m.Path, m => m.Mount));
        }

        [Test]
        public void should_return_ok_when_there_are_no_movies()
        {
            Subject.Check().ShouldBeOk();
        }

        [Test]
        public void should_return_ok_when_movies_are_on_writable_mounts()
        {
            GivenMovies((_moviePath, _writableMount), (_otherMoviePath, _writableMount));

            Subject.Check().ShouldBeOk();
        }

        [Test]
        public void should_return_ok_when_mount_of_movie_is_unknown()
        {
            GivenMovies((_moviePath, null));

            Subject.Check().ShouldBeOk();
        }

        [Test]
        public void should_return_ok_when_mount_options_of_movie_are_unknown()
        {
            GivenMovies((_moviePath, CreateMount("/dev/sda1", @"C:\".AsOsAgnostic(), null)));

            Subject.Check().ShouldBeOk();
        }

        [Test]
        public void should_return_error_when_movie_is_on_read_only_mount()
        {
            GivenMovies((_moviePath, _writableMount), (_readOnlyMoviePath, _readOnlyMount));

            Subject.Check().ShouldBeError($"/dev/sdb1 ({_readOnlyMoviePath})", "#movies-mount-ro");
        }

        [Test]
        public void should_list_read_only_mount_once_with_the_path_of_its_first_movie()
        {
            GivenMovies((_readOnlyMoviePath, _readOnlyMount), (_otherReadOnlyMoviePath, _readOnlyMount));

            var result = Subject.Check();

            result.ShouldBeError($"read-only: /dev/sdb1 ({_readOnlyMoviePath})");
            result.Message.Should().NotContain(_otherReadOnlyMoviePath);
        }

        [Test]
        public void should_get_the_mounts_of_all_movie_paths_at_once()
        {
            GivenMovies((_moviePath, _writableMount), (_otherMoviePath, _writableMount), (_readOnlyMoviePath, _readOnlyMount));

            Subject.Check();

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.GetMountsForPaths(It.Is<HashSet<string>>(p => p.SetEquals(new[] { _moviePath, _otherMoviePath, _readOnlyMoviePath }))), Times.Once());

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.GetMount(It.IsAny<string>()), Times.Never());
        }
    }
}
