using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.MovieImport.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.MovieImport.Specifications
{
    [TestFixture]
    public class NotMultiDiscSpecificationFixture : CoreTest<NotMultiDiscSpecification>
    {
        private LocalMovie _localMovie;

        [SetUp]
        public void Setup()
        {
            _localMovie = new LocalMovie
            {
                Path = @"C:\Test\Downloaded\30.rock.s01e01.avi".AsOsAgnostic()
            };
        }

        [TestCase(@"C:\Test\Downloaded\x.men.2018.avi")]
        [TestCase(@"C:\Test\Downloaded\Bad Boys (2006).mkv")]
        [TestCase(@"C:\Test\Downloaded\Bad Boys 2006.mkv")]
        [TestCase(@"C:\Test\Downloaded\300 (2006).mkv")]
        [TestCase(@"C:\Test\Downloaded\300 2006.mkv")]
        [TestCase(@"C:\Test\Downloaded\Star Trek 1 - The motion picture.mkv")]
        [TestCase(@"C:\Test\Downloaded\Red Riding in the Year of Our Lord 1983 (2009).mkv")]
        public void should_be_accepted_for_legitimate_files(string path)
        {
            _localMovie.Path = path.AsOsAgnostic();

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }

        [TestCase(@"C:\Test\Downloaded\Bad Boys (2006) part1.mkv")]
        [TestCase(@"C:\Test\Downloaded\300 2006 part1.mkv")]
        [TestCase(@"C:\Test\Downloaded\Bad Boys (2006).part1.stv.unrated.multi.1080p.bluray.x264-rough.mkv")]
        [TestCase(@"C:\Test\Downloaded\Bad Boys (2006) parta.mkv")]
        [TestCase(@"C:\Test\Downloaded\blah blah - cd 1.mvk")]
        [TestCase(@"C:\Test\Downloaded\Millennium.Part.1-Män.Som.Hatar.Kvinnor.2009.Extended.Cut.1080p.BluRay.DTS.x264-CtrlHD.mkv")]
        public void should_be_rejected_for_multi_part_files(string path)
        {
            _localMovie.Path = path.AsOsAgnostic();

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeFalse();
        }
    }
}
