﻿using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Crypto;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Download.Pending.PendingReleaseServiceTests
{
    [TestFixture]
	[Ignore("Series")]
    public class RemovePendingFixture : CoreTest<PendingReleaseService>
    {
        private List<PendingRelease> _pending;
        private Episode _episode;

        [SetUp]
        public void Setup()
        {
            _pending = new List<PendingRelease>();

            _episode = Builder<Episode>.CreateNew()
                                       .Build();

            Mocker.GetMock<IPendingReleaseRepository>()
                 .Setup(s => s.AllBySeriesId(It.IsAny<int>()))
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

        private void AddPending(int id, int seasonNumber, int[] episodes)
        {
            _pending.Add(new PendingRelease
             {
                 Id = id,
                 ParsedEpisodeInfo = new ParsedEpisodeInfo { SeasonNumber = seasonNumber, EpisodeNumbers = episodes }
             });
        }

        [Test]
        public void should_remove_same_release()
        {
            AddPending(id: 1, seasonNumber: 2, episodes: new[] { 3 });

            var queueId = HashConverter.GetHashInt31(string.Format("pending-{0}-ep{1}", 1, _episode.Id));

            Subject.RemovePendingQueueItems(queueId);

            AssertRemoved(1);
        }
        
        [Test]
        public void should_remove_multiple_releases_release()
        {
            AddPending(id: 1, seasonNumber: 2, episodes: new[] { 1 });
            AddPending(id: 2, seasonNumber: 2, episodes: new[] { 2 });
            AddPending(id: 3, seasonNumber: 2, episodes: new[] { 3 });
            AddPending(id: 4, seasonNumber: 2, episodes: new[] { 3 });

            var queueId = HashConverter.GetHashInt31(string.Format("pending-{0}-ep{1}", 3, _episode.Id));

            Subject.RemovePendingQueueItems(queueId);

            AssertRemoved(3, 4);
        }

        [Test]
        public void should_not_remove_diffrent_season()
        {
            AddPending(id: 1, seasonNumber: 2, episodes: new[] { 1 });
            AddPending(id: 2, seasonNumber: 2, episodes: new[] { 1 });
            AddPending(id: 3, seasonNumber: 3, episodes: new[] { 1 });
            AddPending(id: 4, seasonNumber: 3, episodes: new[] { 1 });

            var queueId = HashConverter.GetHashInt31(string.Format("pending-{0}-ep{1}", 1, _episode.Id));

            Subject.RemovePendingQueueItems(queueId);

            AssertRemoved(1, 2);
        }

        [Test]
        public void should_not_remove_diffrent_episodes()
        {
            AddPending(id: 1, seasonNumber: 2, episodes: new[] { 1 });
            AddPending(id: 2, seasonNumber: 2, episodes: new[] { 1 });
            AddPending(id: 3, seasonNumber: 2, episodes: new[] { 2 });
            AddPending(id: 4, seasonNumber: 2, episodes: new[] { 3 });

            var queueId = HashConverter.GetHashInt31(string.Format("pending-{0}-ep{1}", 1, _episode.Id));

            Subject.RemovePendingQueueItems(queueId);

            AssertRemoved(1, 2);
        }

        [Test]
        public void should_not_remove_multiepisodes()
        {
            AddPending(id: 1, seasonNumber: 2, episodes: new[] { 1 });
            AddPending(id: 2, seasonNumber: 2, episodes: new[] { 1, 2 });

            var queueId = HashConverter.GetHashInt31(string.Format("pending-{0}-ep{1}", 1, _episode.Id));

            Subject.RemovePendingQueueItems(queueId);

            AssertRemoved(1);
        }

        [Test]
        public void should_not_remove_singleepisodes()
        {
            AddPending(id: 1, seasonNumber: 2, episodes: new[] { 1 });
            AddPending(id: 2, seasonNumber: 2, episodes: new[] { 1, 2 });

            var queueId = HashConverter.GetHashInt31(string.Format("pending-{0}-ep{1}", 2, _episode.Id));

            Subject.RemovePendingQueueItems(queueId);

            AssertRemoved(2);
        }
        
        private void AssertRemoved(params int[] ids)
        {
            Mocker.GetMock<IPendingReleaseRepository>().Verify(c => c.DeleteMany(It.Is<IEnumerable<int>>(s => s.SequenceEqual(ids))));
        }
    }

}
