using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Movies;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.MediaFileServiceTests
{
    [TestFixture]
    public class FilterFixture : CoreTest<MediaFileService>
    {
        private Movie _series;

        [SetUp]
        public void Setup()
        {
            _series = new Movie
                      {
                          Id = 10,
                          Path = @"C:\".AsOsAgnostic()
                      };
        }

        [Test]
        public void filter_should_return_all_files_if_no_existing_files()
        {
            var files = new List<string>()
            {
                "C:\\file1.avi".AsOsAgnostic(),
                "C:\\file2.avi".AsOsAgnostic(),
                "C:\\file3.avi".AsOsAgnostic()
            };

            Mocker.GetMock<IMediaFileRepository>()
                .Setup(c => c.GetFilesByMovie(It.IsAny<int>()))
                .Returns(new List<MovieFile>());


            Subject.FilterExistingFiles(files, _series).Should().BeEquivalentTo(files);
        }

        [Test]
        public void filter_should_return_none_if_all_files_exist()
        {
            var files = new List<string>()
            {
                "C:\\file1.avi".AsOsAgnostic(),
                "C:\\file2.avi".AsOsAgnostic(),
                "C:\\file3.avi".AsOsAgnostic()
            };

            Mocker.GetMock<IMediaFileRepository>()
                .Setup(c => c.GetFilesByMovie(It.IsAny<int>()))
                .Returns(files.Select(f => new MovieFile { RelativePath = Path.GetFileName(f) }).ToList());


            Subject.FilterExistingFiles(files, _series).Should().BeEmpty();
        }

        [Test]
        public void filter_should_return_none_existing_files()
        {
            var files = new List<string>()
            {
                "C:\\file1.avi".AsOsAgnostic(),
                "C:\\file2.avi".AsOsAgnostic(),
                "C:\\file3.avi".AsOsAgnostic()
            };

            Mocker.GetMock<IMediaFileRepository>()
                .Setup(c => c.GetFilesByMovie(It.IsAny<int>()))
                .Returns(new List<MovieFile>
                {
                    new MovieFile{ RelativePath = "file2.avi".AsOsAgnostic()}
                });


            Subject.FilterExistingFiles(files, _series).Should().HaveCount(2);
            Subject.FilterExistingFiles(files, _series).Should().NotContain("C:\\file2.avi".AsOsAgnostic());
        }

        [Test]
        public void filter_should_return_none_existing_files_ignoring_case()
        {
            WindowsOnly();

            var files = new List<string>()
            {
                "C:\\file1.avi".AsOsAgnostic(),
                "C:\\FILE2.avi".AsOsAgnostic(),
                "C:\\file3.avi".AsOsAgnostic()
            };

            Mocker.GetMock<IMediaFileRepository>()
                .Setup(c => c.GetFilesByMovie(It.IsAny<int>()))
                .Returns(new List<MovieFile>
                {
                    new MovieFile{ RelativePath = "file2.avi".AsOsAgnostic()}
                });


            Subject.FilterExistingFiles(files, _series).Should().HaveCount(2);
            Subject.FilterExistingFiles(files, _series).Should().NotContain("C:\\file2.avi".AsOsAgnostic());
        }

        [Test]
        public void filter_should_return_none_existing_files_not_ignoring_case()
        {
            PosixOnly();

            var files = new List<string>()
            {
                "C:\\file1.avi".AsOsAgnostic(),
                "C:\\FILE2.avi".AsOsAgnostic(),
                "C:\\file3.avi".AsOsAgnostic()
            };

            Mocker.GetMock<IMediaFileRepository>()
                .Setup(c => c.GetFilesByMovie(It.IsAny<int>()))
                .Returns(new List<MovieFile>
                {
                    new MovieFile{ RelativePath = "file2.avi".AsOsAgnostic()}
                });

            Subject.FilterExistingFiles(files, _series).Should().HaveCount(3);
        }

        [Test]
        public void filter_should_not_change_casing()
        {
            var files = new List<string>()
            {
                "C:\\FILE1.avi".AsOsAgnostic()
            };

            Mocker.GetMock<IMediaFileRepository>()
                .Setup(c => c.GetFilesByMovie(It.IsAny<int>()))
                .Returns(new List<MovieFile>());

            Subject.FilterExistingFiles(files, _series).Should().HaveCount(1);
            Subject.FilterExistingFiles(files, _series).Should().NotContain(files.First().ToLower());
            Subject.FilterExistingFiles(files, _series).Should().Contain(files.First());
        }
    }
}
