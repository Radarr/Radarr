using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Crypto;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Download.Pending.PendingReleaseServiceTests
{
    [TestFixture]
    public class RemovePendingFixture : CoreTest<PendingReleaseService>
    {
        private List<PendingRelease> _pending;
        private Movie _movie;

        [SetUp]
        public void Setup()
        {
            _pending = new List<PendingRelease>();

            _movie = Builder<Movie>.CreateNew()
                                       .Build();

            Mocker.GetMock<IPendingReleaseRepository>()
                 .Setup(s => s.AllByMovieId(It.IsAny<int>()))
                 .Returns(_pending);

            Mocker.GetMock<IPendingReleaseRepository>()
                  .Setup(s => s.All())
                  .Returns(_pending);

            Mocker.GetMock<IMovieService>()
                  .Setup(s => s.GetMovie(It.IsAny<int>()))
                  .Returns(_movie);

            Mocker.GetMock<IMovieService>()
                  .Setup(s => s.GetMovies(It.IsAny<IEnumerable<int>>()))
                  .Returns(new List<Movie> { _movie });

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.GetMovie(It.IsAny<string>()))
                  .Returns(_movie);
        }

        private void AddPending(int id, string title, int year)
        {
            _pending.Add(new PendingRelease
            {
                Id = id,
                ParsedMovieInfo = new ParsedMovieInfo { MovieTitles = new List<string> { title }, Year = year },
                MovieId = _movie.Id
            });
        }

        [Test]
        public void should_remove_same_release()
        {
            AddPending(id: 1, title: "Movie", year: 2001);

            var queueId = HashConverter.GetHashInt31(string.Format("pending-{0}-movie{1}", 1, _movie.Id));

            Subject.RemovePendingQueueItems(queueId);

            AssertRemoved(1);
        }

        private void AssertRemoved(params int[] ids)
        {
            Mocker.GetMock<IPendingReleaseRepository>().Verify(c => c.DeleteMany(It.Is<IEnumerable<int>>(s => s.SequenceEqual(ids))));
        }
    }
}
