using System;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Books;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles.BookImport.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.BookImport.Specifications
{
    [TestFixture]
    public class NotUnpackingSpecificationFixture : CoreTest<NotUnpackingSpecification>
    {
        private LocalBook _localTrack;

        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IConfigService>()
                .SetupGet(s => s.DownloadClientWorkingFolders)
                .Returns("_UNPACK_|_FAILED_");

            _localTrack = new LocalBook
            {
                Path = @"C:\Test\Unsorted Music\Kid.Rock\Kid.Rock.Cowboy.mp3".AsOsAgnostic(),
                Size = 100,
                Author = Builder<Author>.CreateNew().Build()
            };
        }

        private void GivenInWorkingFolder()
        {
            _localTrack.Path = @"C:\Test\Unsorted Music\_UNPACK_Kid.Rock\someSubFolder\Kid.Rock.Cowboy.mp3".AsOsAgnostic();
        }

        private void GivenLastWriteTimeUtc(DateTime time)
        {
            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.FileGetLastWrite(It.IsAny<string>()))
                .Returns(time);
        }

        [Test]
        public void should_return_true_if_not_in_working_folder()
        {
            Subject.IsSatisfiedBy(_localTrack, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_in_old_working_folder()
        {
            WindowsOnly();

            GivenInWorkingFolder();
            GivenLastWriteTimeUtc(DateTime.UtcNow.AddHours(-1));

            Subject.IsSatisfiedBy(_localTrack, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_in_working_folder_and_last_write_time_was_recent()
        {
            GivenInWorkingFolder();
            GivenLastWriteTimeUtc(DateTime.UtcNow);

            Subject.IsSatisfiedBy(_localTrack, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_unopacking_on_linux()
        {
            PosixOnly();

            GivenInWorkingFolder();
            GivenLastWriteTimeUtc(DateTime.UtcNow.AddDays(-5));

            Subject.IsSatisfiedBy(_localTrack, null).Accepted.Should().BeFalse();
        }
    }
}
