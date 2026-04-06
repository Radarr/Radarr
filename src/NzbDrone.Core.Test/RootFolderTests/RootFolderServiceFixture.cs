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
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.RootFolderTests
{
    [TestFixture]
    public class RootFolderServiceFixture : CoreTest<RootFolderService>
    {
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _namingConfig = NamingConfig.Default;

            Mocker.GetMock<IDiskProvider>()
                  .Setup(m => m.FolderExists(It.IsAny<string>()))
                  .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(m => m.FolderWritable(It.IsAny<string>()))
                  .Returns(true);

            Mocker.GetMock<IRootFolderRepository>()
                  .Setup(s => s.All())
                  .Returns(new List<RootFolder>());

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig())
                  .Returns(_namingConfig);
        }

        private void WithNonExistingFolder()
        {
            Mocker.GetMock<IDiskProvider>()
                .Setup(m => m.FolderExists(It.IsAny<string>()))
                .Returns(false);
        }

        private void GivenRootFolder(RootFolder rootFolder)
        {
            Mocker.GetMock<IRootFolderRepository>()
                .Setup(s => s.Get(It.IsAny<int>()))
                .Returns(rootFolder);

            Mocker.GetMock<IMovieRepository>()
                .Setup(s => s.AllMoviePaths())
                .Returns(new Dictionary<int, string>());
        }

        [TestCase("D:\\TV Shows\\")]
        [TestCase("//server//folder")]
        public void should_be_able_to_add_root_dir(string path)
        {
            Mocker.GetMock<IMovieRepository>()
                  .Setup(s => s.AllMoviePaths())
                  .Returns(new Dictionary<int, string>());

            var root = new RootFolder { Path = path.AsOsAgnostic() };

            Subject.Add(root);

            Mocker.GetMock<IRootFolderRepository>().Verify(c => c.Insert(root), Times.Once());
        }

        [Test]
        public void should_throw_if_folder_being_added_doesnt_exist()
        {
            WithNonExistingFolder();

            Assert.Throws<DirectoryNotFoundException>(() => Subject.Add(new RootFolder { Path = "C:\\TEST".AsOsAgnostic() }));
        }

        [Test]
        public void should_be_able_to_remove_root_dir()
        {
            Subject.Remove(1);
            Mocker.GetMock<IRootFolderRepository>().Verify(c => c.Delete(1), Times.Once());
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("BAD PATH")]
        public void invalid_folder_path_throws_on_add(string path)
        {
            Assert.Throws<ArgumentException>(() =>
                    Mocker.Resolve<RootFolderService>().Add(new RootFolder { Id = 0, Path = path }));
        }

        [Test]
        public void adding_duplicated_root_folder_should_throw()
        {
            Mocker.GetMock<IRootFolderRepository>().Setup(c => c.All()).Returns(new List<RootFolder> { new RootFolder { Path = "C:\\TV".AsOsAgnostic() } });

            Assert.Throws<InvalidOperationException>(() => Subject.Add(new RootFolder { Path = @"C:\TV".AsOsAgnostic() }));
        }

        [Test]
        public void should_throw_when_adding_not_writable_folder()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(m => m.FolderWritable(It.IsAny<string>()))
                  .Returns(false);

            Assert.Throws<UnauthorizedAccessException>(() => Subject.Add(new RootFolder { Path = @"C:\TV".AsOsAgnostic() }));
        }

        [TestCase("$recycle.bin")]
        [TestCase("system volume information")]
        [TestCase("recycler")]
        [TestCase("lost+found")]
        [TestCase(".appledb")]
        [TestCase(".appledesktop")]
        [TestCase(".appledouble")]
        [TestCase("@eadir")]
        [TestCase(".grab")]
        public void should_get_root_folder_with_subfolders_excluding_special_sub_folders(string subFolder)
        {
            var rootFolder = Builder<RootFolder>.CreateNew()
                                                .With(r => r.Path = @"C:\Test\TV")
                                                .Build();
            if (OsInfo.IsNotWindows)
            {
                rootFolder = Builder<RootFolder>.CreateNew()
                                                .With(r => r.Path = @"/Test/TV")
                                                .Build();
            }

            var subFolders = new[]
                        {
                            "Series1",
                            "Series2",
                            "Series3",
                            subFolder
                        };

            var folders = subFolders.Select(f => Path.Combine(@"C:\Test\TV", f)).ToArray();

            if (OsInfo.IsNotWindows)
            {
                folders = subFolders.Select(f => Path.Combine(@"/Test/TV", f)).ToArray();
            }

            Mocker.GetMock<IRootFolderRepository>()
                  .Setup(s => s.Get(It.IsAny<int>()))
                  .Returns(rootFolder);

            Mocker.GetMock<IMovieRepository>()
                  .Setup(s => s.AllMoviePaths())
                  .Returns(new Dictionary<int, string>());

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetDirectories(rootFolder.Path))
                  .Returns(folders);

            var unmappedFolders = Subject.Get(rootFolder.Id, true).UnmappedFolders;

            unmappedFolders.Count.Should().BeGreaterThan(0);
            unmappedFolders.Should().NotContain(u => u.Name == subFolder);
        }

        [TestCase("")]
        [TestCase(null)]
        public void should_handle_non_configured_recycle_bin(string recycleBinPath)
        {
            var rootFolder = Builder<RootFolder>.CreateNew()
                .With(r => r.Path = @"C:\Test\TV")
                .Build();
            if (OsInfo.IsNotWindows)
            {
                rootFolder = Builder<RootFolder>.CreateNew()
                    .With(r => r.Path = @"/Test/TV")
                    .Build();
            }

            var subFolders = new[]
            {
                "Series1",
                "Series2",
                "Series3"
            };

            var folders = subFolders.Select(f => Path.Combine(@"C:\Test\TV", f)).ToArray();

            if (OsInfo.IsNotWindows)
            {
                folders = subFolders.Select(f => Path.Combine(@"/Test/TV", f)).ToArray();
            }

            Mocker.GetMock<IConfigService>()
                .Setup(s => s.RecycleBin)
                .Returns(recycleBinPath);

            Mocker.GetMock<IRootFolderRepository>()
                .Setup(s => s.Get(It.IsAny<int>()))
                .Returns(rootFolder);

            Mocker.GetMock<IMovieRepository>()
                .Setup(s => s.AllMoviePaths())
                .Returns(new Dictionary<int, string>());

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.GetDirectories(rootFolder.Path))
                .Returns(folders);

            var unmappedFolders = Subject.Get(rootFolder.Id, true).UnmappedFolders;

            unmappedFolders.Count.Should().Be(3);
        }

        [Test]
        public void should_exclude_recycle_bin()
        {
            var rootFolder = Builder<RootFolder>.CreateNew()
                .With(r => r.Path = @"C:\Test\TV")
                .Build();

            if (OsInfo.IsNotWindows)
            {
                rootFolder = Builder<RootFolder>.CreateNew()
                    .With(r => r.Path = @"/Test/TV")
                    .Build();
            }

            var subFolders = new[]
            {
                "Series1",
                "Series2",
                "Series3",
                "BIN"
            };

            var folders = subFolders.Select(f => Path.Combine(@"C:\Test\TV", f)).ToArray();

            if (OsInfo.IsNotWindows)
            {
                folders = subFolders.Select(f => Path.Combine(@"/Test/TV", f)).ToArray();
            }

            var recycleFolder = Path.Combine(OsInfo.IsNotWindows ? @"/Test/TV" : @"C:\Test\TV", "BIN");

            Mocker.GetMock<IConfigService>()
                .Setup(s => s.RecycleBin)
                .Returns(recycleFolder);

            Mocker.GetMock<IRootFolderRepository>()
                .Setup(s => s.Get(It.IsAny<int>()))
                .Returns(rootFolder);

            Mocker.GetMock<IMovieRepository>()
                .Setup(s => s.AllMoviePaths())
                .Returns(new Dictionary<int, string>());

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.GetDirectories(rootFolder.Path))
                .Returns(folders);

            var unmappedFolders = Subject.Get(rootFolder.Id, true).UnmappedFolders;

            unmappedFolders.Count.Should().Be(3);
            unmappedFolders.Should().NotContain(u => u.Name == "BIN");
        }

        [Test]
        public void should_get_unmapped_folders_inside_letter_subfolder()
        {
            _namingConfig.MovieFolderFormat = "{Movie TitleFirstCharacter}\\{Movie Title}".AsOsAgnostic();

            var rootFolderPath = @"C:\Test\Movies".AsOsAgnostic();
            var rootFolder = Builder<RootFolder>.CreateNew()
                .With(r => r.Path = rootFolderPath)
                .Build();

            var subFolderPath = Path.Combine(rootFolderPath, "M");

            var subFolders = new[]
            {
                "Movie 1 (2001)",
                "Movie 2 (2002)",
                "Movie 3 (2003)",
            };

            var folders = subFolders.Select(f => Path.Combine(subFolderPath, f)).ToArray();

            GivenRootFolder(rootFolder);

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.GetDirectories(rootFolder.Path))
                .Returns(new[] { subFolderPath });

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.GetDirectories(subFolderPath))
                .Returns(folders);

            var unmappedFolders = Subject.Get(rootFolder.Id, false).UnmappedFolders;

            unmappedFolders.Count.Should().Be(3);
        }

        [Test]
        public void should_get_top_level_movie_folders()
        {
            var rootFolderPath = @"C:\Test\Movies".AsOsAgnostic();
            var rootFolder = Builder<RootFolder>.CreateNew()
                .With(r => r.Path = rootFolderPath)
                .Build();

            var movieFolders = new[]
            {
                "Movie 1 (2001)",
                "Movie 2 (2002)",
                "Movie 3 (2003)",
            };

            var folders = movieFolders.Select(f => Path.Combine(rootFolderPath, f)).ToArray();

            GivenRootFolder(rootFolder);

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.GetDirectories(rootFolder.Path))
                .Returns(folders);

            foreach (var folder in folders)
            {
                Mocker.GetMock<IDiskProvider>()
                    .Setup(s => s.GetDirectories(folder))
                    .Returns(Array.Empty<string>());
            }

            var unmappedFolders = Subject.Get(rootFolder.Id, false).UnmappedFolders;

            unmappedFolders.Select(f => f.Name).Should().BeEquivalentTo(movieFolders);
        }

        [Test]
        public void should_get_nested_movie_folder_when_parent_is_only_a_grouping_folder()
        {
            var rootFolderPath = @"C:\Test\Movies".AsOsAgnostic();
            var rootFolder = Builder<RootFolder>.CreateNew()
                .With(r => r.Path = rootFolderPath)
                .Build();

            var groupingFolder = Path.Combine(rootFolderPath, "L");
            var movieFolder = Path.Combine(groupingFolder, "Ladder 49 (2004)");
            var movieFile = Path.Combine(movieFolder, "Ladder 49 (2004) [Bluray-1080p].mkv").AsOsAgnostic();

            GivenRootFolder(rootFolder);

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.GetDirectories(rootFolder.Path))
                .Returns(new[] { groupingFolder });

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.GetDirectories(groupingFolder))
                .Returns(new[] { movieFolder });

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.GetDirectories(movieFolder))
                .Returns(Array.Empty<string>());

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.GetFiles(groupingFolder, false))
                .Returns(Array.Empty<string>());

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.GetFiles(movieFolder, false))
                .Returns(new[] { movieFile });

            var unmappedFolders = Subject.Get(rootFolder.Id, false).UnmappedFolders;

            unmappedFolders.Should().HaveCount(1);
            unmappedFolders[0].Name.Should().Be("Ladder 49 (2004)");
            unmappedFolders[0].Path.Should().Be(movieFolder);
            unmappedFolders[0].RelativePath.Should().Be(Path.Combine("L", "Ladder 49 (2004)").AsOsAgnostic());
        }

        [Test]
        public void should_not_return_grouping_folder_when_only_child_folder_contains_video_file()
        {
            var rootFolderPath = @"C:\Test\Movies".AsOsAgnostic();
            var rootFolder = Builder<RootFolder>.CreateNew()
                .With(r => r.Path = rootFolderPath)
                .Build();

            var groupingFolder = Path.Combine(rootFolderPath, "L");
            var movieFolder = Path.Combine(groupingFolder, "Ladder 49 (2004)");
            var movieFile = Path.Combine(movieFolder, "Ladder 49 (2004).mkv").AsOsAgnostic();

            GivenRootFolder(rootFolder);

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.GetDirectories(rootFolder.Path))
                .Returns(new[] { groupingFolder });

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.GetDirectories(groupingFolder))
                .Returns(new[] { movieFolder });

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.GetDirectories(movieFolder))
                .Returns(Array.Empty<string>());

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.GetFiles(groupingFolder, false))
                .Returns(Array.Empty<string>());

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.GetFiles(movieFolder, false))
                .Returns(new[] { movieFile });

            var unmappedFolders = Subject.Get(rootFolder.Id, false).UnmappedFolders;

            unmappedFolders.Should().NotContain(u => u.Name == "L");
        }

        [Test]
        public void should_not_return_already_mapped_nested_movie_folder()
        {
            var rootFolderPath = @"C:\Test\Movies".AsOsAgnostic();
            var rootFolder = Builder<RootFolder>.CreateNew()
                .With(r => r.Path = rootFolderPath)
                .Build();

            var groupingFolder = Path.Combine(rootFolderPath, "L");
            var movieFolder = Path.Combine(groupingFolder, "Ladder 49 (2004)");
            var movieFile = Path.Combine(movieFolder, "Ladder 49 (2004).mkv").AsOsAgnostic();

            Mocker.GetMock<IRootFolderRepository>()
                .Setup(s => s.Get(It.IsAny<int>()))
                .Returns(rootFolder);

            Mocker.GetMock<IMovieRepository>()
                .Setup(s => s.AllMoviePaths())
                .Returns(new Dictionary<int, string> { { 1, movieFolder } });

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.GetDirectories(rootFolder.Path))
                .Returns(new[] { groupingFolder });

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.GetDirectories(groupingFolder))
                .Returns(new[] { movieFolder });

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.GetDirectories(movieFolder))
                .Returns(Array.Empty<string>());

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.GetFiles(groupingFolder, false))
                .Returns(Array.Empty<string>());

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.GetFiles(movieFolder, false))
                .Returns(new[] { movieFile });

            var unmappedFolders = Subject.Get(rootFolder.Id, false).UnmappedFolders;

            unmappedFolders.Should().BeEmpty();
        }

        [Test]
        public void should_not_recurse_into_movie_folder_with_disc_subfolders()
        {
            var rootFolderPath = @"C:\Test\Movies".AsOsAgnostic();
            var rootFolder = Builder<RootFolder>.CreateNew()
                .With(r => r.Path = rootFolderPath)
                .Build();

            var movieFolder = Path.Combine(rootFolderPath, "Movie 1 (2001)");
            var discFolder = Path.Combine(movieFolder, "BDMV");

            GivenRootFolder(rootFolder);

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.GetDirectories(rootFolder.Path))
                .Returns(new[] { movieFolder });

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.GetDirectories(movieFolder))
                .Returns(new[] { discFolder });

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.GetDirectories(discFolder))
                .Returns(Array.Empty<string>());

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.GetFiles(movieFolder, false))
                .Returns(Array.Empty<string>());

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.GetFiles(discFolder, false))
                .Returns(Array.Empty<string>());

            var unmappedFolders = Subject.Get(rootFolder.Id, false).UnmappedFolders;

            unmappedFolders.Should().HaveCount(1);
            unmappedFolders[0].Name.Should().Be("Movie 1 (2001)");
        }
    }
}
