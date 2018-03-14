using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Crypto;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Test.Download.Pending.PendingReleaseServiceTests
{
    [TestFixture]
	[Ignore("Series")]
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
                  .Returns( _pending);

            /*Mocker.GetMock<IMovieService>()
                  .Setup(s => s.GetMovie(It.IsAny<int>()))
                  .Returns(_movie);

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.GetMovie(It.IsAny<string>()))
                  .Returns(_movie);*/
        }

        private void AddPending(int id, string title, int year)
        {
            _pending.Add(new PendingRelease
             {
                 Id = id,
                 ParsedMovieInfo = new ParsedMovieInfo { MovieTitle = title, Year = year }
             });
        }

        [Test]
        public void should_remove_same_release()
        {
            AddPending(id: 1, title: "Movie", year: 2001);

            var queueId = HashConverter.GetHashInt31(string.Format("pending-{0}-ep{1}", 1, _movie.Id));

            Subject.RemovePendingQueueItems(queueId);

            AssertRemoved(1);
        }
        
        [Test]
        public void should_remove_multiple_releases_release()
        {
            AddPending(id: 1, title: "Movie", year: 2001);
            AddPending(id: 2, title: "Movie", year: 2002);
            AddPending(id: 3, title: "Movie", year: 2003);
            AddPending(id: 4, title: "Movie", year: 2003);

            var queueId = HashConverter.GetHashInt31(string.Format("pending-{0}-ep{1}", 3, _movie.Id));

            Subject.RemovePendingQueueItems(queueId);

            AssertRemoved(3, 4);
        }

        [Test]
        public void should_not_remove_diffrent_season()
        {
            AddPending(id: 1, title: "Movie", year: 2001);
            AddPending(id: 2, title: "Movie", year: 2001);
            AddPending(id: 3, title: "Movie", year: 2001);
            AddPending(id: 4, title: "Movie", year: 2001);

            var queueId = HashConverter.GetHashInt31(string.Format("pending-{0}-ep{1}", 1, _movie.Id));

            Subject.RemovePendingQueueItems(queueId);

            AssertRemoved(1, 2);
        }

        [Test]
        public void should_not_remove_diffrent_episodes()
        {
            AddPending(id: 1, title: "Movie", year: 2001);
            AddPending(id: 2, title: "Movie", year: 2001);
            AddPending(id: 3, title: "Movie", year: 2001);
            AddPending(id: 4, title: "Movie", year: 2001);

            var queueId = HashConverter.GetHashInt31(string.Format("pending-{0}-ep{1}", 1, _movie.Id));

            Subject.RemovePendingQueueItems(queueId);

            AssertRemoved(1, 2);
        }

        [Test]
        public void should_not_remove_multiepisodes()
        {
            AddPending(id: 1, title: "Movie", year: 2001);
            AddPending(id: 2, title: "Movie", year: 2001);

            var queueId = HashConverter.GetHashInt31(string.Format("pending-{0}-ep{1}", 1, _movie.Id));

            Subject.RemovePendingQueueItems(queueId);

            AssertRemoved(1);
        }

        [Test]
        public void should_not_remove_singleepisodes()
        {
            AddPending(id: 1, title: "Movie", year: 2001);
            AddPending(id: 2, title: "Movie", year: 2001);

            var queueId = HashConverter.GetHashInt31(string.Format("pending-{0}-ep{1}", 2, _movie.Id));

            Subject.RemovePendingQueueItems(queueId);

            AssertRemoved(2);
        }
        
        private void AssertRemoved(params int[] ids)
        {
            Mocker.GetMock<IPendingReleaseRepository>().Verify(c => c.DeleteMany(It.Is<IEnumerable<int>>(s => s.SequenceEqual(ids))));
        }
    }

}
