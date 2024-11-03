using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.MediaFiles.MovieImport.Specifications;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeImport.Specifications
{
    [TestFixture]
    public class HasAudioTrackSpecificationFixture : CoreTest<HasAudioTrackSpecification>
    {
        private Movie _movie;
        private LocalMovie _localMovie;
        private string _rootFolder;

        [SetUp]
        public void Setup()
        {
             _rootFolder = @"C:\Test\Movies".AsOsAgnostic();

             _movie = Builder<Movie>.CreateNew()
                                     .With(s => s.Path = Path.Combine(_rootFolder, "Movie Title"))
                                     .Build();

             _localMovie = new LocalMovie
                                {
                                    Path = @"C:\Test\Unsorted\Movie Title\movie.title.2000.avi".AsOsAgnostic(),
                                    Movie = _movie
                                };
        }

        [Test]
        public void should_accept_if_media_info_is_null()
        {
            _localMovie.MediaInfo = null;

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_reject_if_audio_stream_count_is_0()
        {
            _localMovie.MediaInfo = Builder<MediaInfoModel>.CreateNew().With(m => m.AudioStreamCount = 0).Build();

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_accept_if_audio_stream_count_is_0()
        {
            _localMovie.MediaInfo = Builder<MediaInfoModel>.CreateNew().With(m => m.AudioStreamCount = 1).Build();

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }
    }
}
