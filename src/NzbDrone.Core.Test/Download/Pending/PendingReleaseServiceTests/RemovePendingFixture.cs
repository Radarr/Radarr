using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Crypto;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Music;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Download.Pending.PendingReleaseServiceTests
{
    [TestFixture]
    public class RemovePendingFixture : CoreTest<PendingReleaseService>
    {
        private List<PendingRelease> _pending;
        private Book _album;

        [SetUp]
        public void Setup()
        {
            _pending = new List<PendingRelease>();

            _album = Builder<Book>.CreateNew()
                                       .Build();

            Mocker.GetMock<IPendingReleaseRepository>()
                 .Setup(s => s.AllByAuthorId(It.IsAny<int>()))
                 .Returns(_pending);

            Mocker.GetMock<IPendingReleaseRepository>()
                  .Setup(s => s.All())
                  .Returns(_pending);

            Mocker.GetMock<IArtistService>()
                  .Setup(s => s.GetArtist(It.IsAny<int>()))
                  .Returns(new Author());

            Mocker.GetMock<IArtistService>()
                  .Setup(s => s.GetArtists(It.IsAny<IEnumerable<int>>()))
                  .Returns(new List<Author> { new Author() });

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.GetAlbums(It.IsAny<ParsedAlbumInfo>(), It.IsAny<Author>(), null))
                  .Returns(new List<Book> { _album });
        }

        private void AddPending(int id, string album)
        {
            _pending.Add(new PendingRelease
            {
                Id = id,
                ParsedAlbumInfo = new ParsedAlbumInfo { AlbumTitle = album }
            });
        }

        [Test]
        public void should_remove_same_release()
        {
            AddPending(id: 1, album: "Album");

            var queueId = HashConverter.GetHashInt31(string.Format("pending-{0}-album{1}", 1, _album.Id));

            Subject.RemovePendingQueueItems(queueId);

            AssertRemoved(1);
        }

        [Test]
        public void should_remove_multiple_releases_release()
        {
            AddPending(id: 1, album: "Album 1");
            AddPending(id: 2, album: "Album 2");
            AddPending(id: 3, album: "Album 3");
            AddPending(id: 4, album: "Album 3");

            var queueId = HashConverter.GetHashInt31(string.Format("pending-{0}-album{1}", 3, _album.Id));

            Subject.RemovePendingQueueItems(queueId);

            AssertRemoved(3, 4);
        }

        [Test]
        public void should_not_remove_diffrent_albums()
        {
            AddPending(id: 1, album: "Album 1");
            AddPending(id: 2, album: "Album 1");
            AddPending(id: 3, album: "Album 2");
            AddPending(id: 4, album: "Album 3");

            var queueId = HashConverter.GetHashInt31(string.Format("pending-{0}-album{1}", 1, _album.Id));

            Subject.RemovePendingQueueItems(queueId);

            AssertRemoved(1, 2);
        }

        private void AssertRemoved(params int[] ids)
        {
            Mocker.GetMock<IPendingReleaseRepository>().Verify(c => c.DeleteMany(It.Is<IEnumerable<int>>(s => s.SequenceEqual(ids))));
        }
    }
}
