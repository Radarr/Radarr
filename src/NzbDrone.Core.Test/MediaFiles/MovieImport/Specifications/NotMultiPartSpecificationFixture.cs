using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles.MovieImport.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.MovieImport.Specifications
{
    [TestFixture]
    public class NotMultiPartSpecificationFixture : CoreTest<NotMultiPartSpecification>
    {
        private LocalMovie _localMovie;

        [SetUp]
        public void Setup()
        {
            _localMovie = new LocalMovie
            {
                Path = @"C:\Test\Downloaded\somemovie.avi".AsOsAgnostic()
            };
        }

        [TestCase(new object[]
        {
            @"C:\Test\Downloaded\x.men.2018.avi"
        })]
        [TestCase(new object[]
        {
            @"C:\Test\Downloaded\Captain.Phillips.2013.MULTi.1080p.BluRay.x264-LOST\lost-captainphillips.2013.1080p.mkv"
        })]
        [TestCase(new object[]
        {
            @"C:\Test\Downloaded\Harry.Potter.And.The.Deathly.Hallows.Part.1.2010.1080p.BluRay.x264-EbP\Harry.Potter.And.The.Deathly.Hallows.Part.1.2010.1080p.BluRay.x264-EbP.mkv"
        })]
        public void should_be_accepted_for_legitimate_files(object[] paths)
        {
            _localMovie.Path = paths.First().ToString().AsOsAgnostic();

            var filePaths = paths.Cast<string>().Select(x => x.AsOsAgnostic()).ToArray();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetFiles(_localMovie.Path.GetParentPath(), false))
                  .Returns(filePaths);

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }

        [TestCase(new object[]
        {
            @"C:\Test\Downloaded\Bad Boys (2006) part1.mkv",
            @"C:\Test\Downloaded\Bad Boys (2006) part2.mkv"
        })]
        [TestCase(new object[]
        {
            @"C:\Test\Downloaded\blah blah - cd 1.mvk",
            @"C:\Test\Downloaded\blah blah - cd 2.mvk"
        })]
        [TestCase(new object[]
        {
            @"C:\Test\Downloaded\blah blah - dvd a.mvk",
            @"C:\Test\Downloaded\blah blah - dvd b.mvk"
        })]
        public void should_be_rejected_for_multi_part_files(object[] paths)
        {
            _localMovie.Path = paths.First().ToString().AsOsAgnostic();

            var filePaths = paths.Cast<string>().Select(x => x.AsOsAgnostic()).ToArray();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetFiles(_localMovie.Path.GetParentPath(), false))
                  .Returns(filePaths);

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeFalse();
        }

        [TestCase(new object[]
        {
            @"C:\Test\Downloaded\blah blah - cd a.mvk",
            @"C:\Test\Downloaded\blah blah - cd 2.mvk"
        })]
        public void should_not_reject_if_multi_part_schemes_mixed(object[] paths)
        {
            _localMovie.Path = paths.First().ToString().AsOsAgnostic();

            var filePaths = paths.Cast<string>().Select(x => x.AsOsAgnostic()).ToArray();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetFiles(_localMovie.Path.GetParentPath(), false))
                  .Returns(filePaths);

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }

        [TestCase(new object[]
        {
            @"C:\Test\Downloaded\blah blah - cd a.mvk",
            @"C:\Test\Downloaded\ping pong - cd b.mvk"
        })]
        public void should_not_reject_if_file_names_mixed(object[] paths)
        {
            _localMovie.Path = paths.First().ToString().AsOsAgnostic();

            var filePaths = paths.Cast<string>().Select(x => x.AsOsAgnostic()).ToArray();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetFiles(_localMovie.Path.GetParentPath(), false))
                  .Returns(filePaths);

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }
    }
}
