using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Music;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.MediaFileServiceTests
{
    [TestFixture]
    public class FilterFixture : CoreTest<MediaFileService>
    {
        private Artist _artist;

        [SetUp]
        public void Setup()
        {
            _artist = new Artist
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
                .Setup(c => c.GetFilesByArtist(It.IsAny<int>()))
                .Returns(new List<TrackFile>());


            Subject.FilterExistingFiles(files, _artist).Should().BeEquivalentTo(files);
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
                .Setup(c => c.GetFilesByArtist(It.IsAny<int>()))
                .Returns(files.Select(f => new TrackFile { RelativePath = Path.GetFileName(f) }).ToList());


            Subject.FilterExistingFiles(files, _artist).Should().BeEmpty();
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
                .Setup(c => c.GetFilesByArtist(It.IsAny<int>()))
                .Returns(new List<TrackFile>
                {
                    new TrackFile{ RelativePath = "file2.avi".AsOsAgnostic()}
                });


            Subject.FilterExistingFiles(files, _artist).Should().HaveCount(2);
            Subject.FilterExistingFiles(files, _artist).Should().NotContain("C:\\file2.avi".AsOsAgnostic());
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
                .Setup(c => c.GetFilesByArtist(It.IsAny<int>()))
                .Returns(new List<TrackFile>
                {
                    new TrackFile{ RelativePath = "file2.avi".AsOsAgnostic()}
                });


            Subject.FilterExistingFiles(files, _artist).Should().HaveCount(2);
            Subject.FilterExistingFiles(files, _artist).Should().NotContain("C:\\file2.avi".AsOsAgnostic());
        }

        [Test]
        public void filter_should_return_none_existing_files_not_ignoring_case()
        {
            MonoOnly();

            var files = new List<string>()
            {
                "C:\\file1.avi".AsOsAgnostic(),
                "C:\\FILE2.avi".AsOsAgnostic(),
                "C:\\file3.avi".AsOsAgnostic()
            };

            Mocker.GetMock<IMediaFileRepository>()
                .Setup(c => c.GetFilesByArtist(It.IsAny<int>()))
                .Returns(new List<TrackFile>
                {
                    new TrackFile{ RelativePath = "file2.avi".AsOsAgnostic()}
                });

            Subject.FilterExistingFiles(files, _artist).Should().HaveCount(3);
        }

        [Test]
        public void filter_should_not_change_casing()
        {
            var files = new List<string>()
            {
                "C:\\FILE1.avi".AsOsAgnostic()
            };

            Mocker.GetMock<IMediaFileRepository>()
                .Setup(c => c.GetFilesByArtist(It.IsAny<int>()))
                .Returns(new List<TrackFile>());

            Subject.FilterExistingFiles(files, _artist).Should().HaveCount(1);
            Subject.FilterExistingFiles(files, _artist).Should().NotContain(files.First().ToLower());
            Subject.FilterExistingFiles(files, _artist).Should().Contain(files.First());
        }
    }
}