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
    public class MatchesFolderSpecificationFixture : CoreTest<MatchesFolderSpecification>
    {
        private LocalMovie _localMovie;

        [SetUp]
        public void Setup()
        {
            _localMovie = Builder<LocalMovie>.CreateNew()
                                                 .With(l => l.Path = @"C:\Test\Unsorted\Series.Title.S01E01.720p.HDTV-Sonarr\S01E05.mkv".AsOsAgnostic())
                                                 .With(l => l.ParsedMovieInfo =
                                                     Builder<ParsedMovieInfo>.CreateNew()
                                                                               .Build())
                                                 .Build();
        }

        [Test]
        public void should_be_accepted_for_existing_file()
        {
            _localMovie.ExistingFile = true;

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_folder_name_is_not_parseable()
        {
            _localMovie.Path = @"C:\Test\Unsorted\Series.Title\S01E01.mkv".AsOsAgnostic();

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_should_be_accepted_for_full_season()
        {
            _localMovie.Path = @"C:\Test\Unsorted\Series.Title.S01\S01E01.mkv".AsOsAgnostic();

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_file_and_folder_have_the_same_episode()
        {
            _localMovie.Path = @"C:\Test\Unsorted\Series.Title.S01E01.720p.HDTV-Sonarr\S01E01.mkv".AsOsAgnostic();
            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }


        [Test]
        public void should_be_rejected_if_file_and_folder_do_not_have_same_episode()
        {
            _localMovie.Path = @"C:\Test\Unsorted\Series.Title.S01E01.720p.HDTV-Sonarr\S01E05.mkv".AsOsAgnostic();
            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeFalse();            
        }

    }
}
