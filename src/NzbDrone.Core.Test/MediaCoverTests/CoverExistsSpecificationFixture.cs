using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Test.Framework;
using System;

namespace NzbDrone.Core.Test.MediaCoverTests
{
    [TestFixture]
    public class CoverAlreadyExistsSpecificationFixture : CoreTest<CoverAlreadyExistsSpecification>
    {
        private void GivenFileExistsOnDisk()
        {
            Mocker.GetMock<IDiskProvider>().Setup(c => c.FileExists(It.IsAny<string>())).Returns(true);
        }


        private void GivenExistingFileDate(DateTime lastModifiedDate)
        {
            GivenFileExistsOnDisk();
            Mocker.GetMock<IDiskProvider>().Setup(c => c.FileGetLastWrite(It.IsAny<string>())).Returns(lastModifiedDate);

        }

        [Test]
        public void should_return_false_if_file_not_exists()
        {
            Subject.AlreadyExists(DateTime.Now, "c:\\file.exe").Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_file_exists_but_diffrent_date()
        {
            GivenExistingFileDate(DateTime.Now);

            Subject.AlreadyExists(DateTime.Now.AddHours(-5), "c:\\file.exe").Should().BeFalse();
        }

        [Test]
        public void should_return_ture_if_file_exists_and_same_date()
        {
            var givenDate = DateTime.Now;

            GivenExistingFileDate(givenDate);

            Subject.AlreadyExists(givenDate, "c:\\file.exe").Should().BeTrue();
        }
    }
}
