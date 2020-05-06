using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.ImportLists.Exclusions;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Music;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ImportListTests
{
    public class ImportListSyncServiceFixture : CoreTest<ImportListSyncService>
    {
        private List<ImportListItemInfo> _importListReports;

        [SetUp]
        public void SetUp()
        {
            var importListItem1 = new ImportListItemInfo
            {
                Artist = "Linkin Park"
            };

            _importListReports = new List<ImportListItemInfo> { importListItem1 };

            Mocker.GetMock<IFetchAndParseImportList>()
                .Setup(v => v.Fetch())
                .Returns(_importListReports);

            Mocker.GetMock<ISearchForNewAuthor>()
                .Setup(v => v.SearchForNewAuthor(It.IsAny<string>()))
                .Returns(new List<Author>());

            Mocker.GetMock<ISearchForNewBook>()
                .Setup(v => v.SearchForNewBook(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new List<Book>());

            Mocker.GetMock<ISearchForNewBook>()
                .Setup(v => v.SearchByGoodreadsId(It.IsAny<int>()))
                .Returns<int>(x => Builder<Book>
                              .CreateListOfSize(1)
                              .TheFirst(1)
                              .With(b => b.GoodreadsId = x)
                              .With(b => b.ForeignBookId = x.ToString())
                              .BuildList());

            Mocker.GetMock<IImportListFactory>()
                .Setup(v => v.Get(It.IsAny<int>()))
                .Returns(new ImportListDefinition { ShouldMonitor = ImportListMonitorType.SpecificAlbum });

            Mocker.GetMock<IFetchAndParseImportList>()
                .Setup(v => v.Fetch())
                .Returns(_importListReports);

            Mocker.GetMock<IImportListExclusionService>()
                .Setup(v => v.All())
                .Returns(new List<ImportListExclusion>());

            Mocker.GetMock<IAddAlbumService>()
                .Setup(v => v.AddAlbums(It.IsAny<List<Book>>(), false))
                .Returns<List<Book>, bool>((x, y) => x);

            Mocker.GetMock<IAddArtistService>()
                .Setup(v => v.AddArtists(It.IsAny<List<Author>>(), false))
                .Returns<List<Author>, bool>((x, y) => x);
        }

        private void WithAlbum()
        {
            _importListReports.First().Album = "Meteora";
        }

        private void WithAuthorId()
        {
            _importListReports.First().ArtistMusicBrainzId = "f59c5520-5f46-4d2c-b2c4-822eabf53419";
        }

        private void WithBookId()
        {
            _importListReports.First().AlbumMusicBrainzId = "101";
        }

        private void WithExistingArtist()
        {
            Mocker.GetMock<IArtistService>()
                .Setup(v => v.FindById(_importListReports.First().ArtistMusicBrainzId))
                .Returns(new Author { ForeignAuthorId = _importListReports.First().ArtistMusicBrainzId });
        }

        private void WithExistingAlbum()
        {
            Mocker.GetMock<IAlbumService>()
                .Setup(v => v.FindById(_importListReports.First().AlbumMusicBrainzId))
                .Returns(new Book { ForeignBookId = _importListReports.First().AlbumMusicBrainzId });
        }

        private void WithExcludedArtist()
        {
            Mocker.GetMock<IImportListExclusionService>()
                .Setup(v => v.All())
                .Returns(new List<ImportListExclusion>
                {
                    new ImportListExclusion
                    {
                        ForeignId = "f59c5520-5f46-4d2c-b2c4-822eabf53419"
                    }
                });
        }

        private void WithExcludedAlbum()
        {
            Mocker.GetMock<IImportListExclusionService>()
                .Setup(v => v.All())
                .Returns(new List<ImportListExclusion>
                {
                    new ImportListExclusion
                    {
                        ForeignId = "101"
                    }
                });
        }

        private void WithMonitorType(ImportListMonitorType monitor)
        {
            Mocker.GetMock<IImportListFactory>()
                .Setup(v => v.Get(It.IsAny<int>()))
                .Returns(new ImportListDefinition { ShouldMonitor = monitor });
        }

        [Test]
        public void should_search_if_artist_title_and_no_artist_id()
        {
            Subject.Execute(new ImportListSyncCommand());

            Mocker.GetMock<ISearchForNewAuthor>()
                .Verify(v => v.SearchForNewAuthor(It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_not_search_if_artist_title_and_artist_id()
        {
            WithAuthorId();
            Subject.Execute(new ImportListSyncCommand());

            Mocker.GetMock<ISearchForNewAuthor>()
                .Verify(v => v.SearchForNewAuthor(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_search_if_album_title_and_no_album_id()
        {
            WithAlbum();
            Subject.Execute(new ImportListSyncCommand());

            Mocker.GetMock<ISearchForNewBook>()
                .Verify(v => v.SearchForNewBook(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_not_search_if_album_title_and_album_id()
        {
            WithAuthorId();
            WithBookId();
            Subject.Execute(new ImportListSyncCommand());

            Mocker.GetMock<ISearchForNewBook>()
                .Verify(v => v.SearchForNewBook(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_not_search_if_all_info()
        {
            WithAuthorId();
            WithAlbum();
            WithBookId();
            Subject.Execute(new ImportListSyncCommand());

            Mocker.GetMock<ISearchForNewAuthor>()
                .Verify(v => v.SearchForNewAuthor(It.IsAny<string>()), Times.Never());

            Mocker.GetMock<ISearchForNewBook>()
                .Verify(v => v.SearchForNewBook(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_not_add_if_existing_artist()
        {
            WithAuthorId();
            WithExistingArtist();

            Subject.Execute(new ImportListSyncCommand());

            Mocker.GetMock<IAddArtistService>()
                .Verify(v => v.AddArtists(It.Is<List<Author>>(t => t.Count == 0), false));
        }

        [Test]
        public void should_not_add_if_existing_album()
        {
            WithBookId();
            WithExistingAlbum();

            Subject.Execute(new ImportListSyncCommand());

            Mocker.GetMock<IAddArtistService>()
                .Verify(v => v.AddArtists(It.Is<List<Author>>(t => t.Count == 0), false));
        }

        [Test]
        public void should_add_if_existing_artist_but_new_album()
        {
            WithBookId();
            WithExistingArtist();

            Subject.Execute(new ImportListSyncCommand());

            Mocker.GetMock<IAddAlbumService>()
                .Verify(v => v.AddAlbums(It.Is<List<Book>>(t => t.Count == 1), false));
        }

        [TestCase(ImportListMonitorType.None, false)]
        [TestCase(ImportListMonitorType.SpecificAlbum, true)]
        [TestCase(ImportListMonitorType.EntireArtist, true)]
        public void should_add_if_not_existing_artist(ImportListMonitorType monitor, bool expectedArtistMonitored)
        {
            WithAuthorId();
            WithMonitorType(monitor);

            Subject.Execute(new ImportListSyncCommand());

            Mocker.GetMock<IAddArtistService>()
                .Verify(v => v.AddArtists(It.Is<List<Author>>(t => t.Count == 1 && t.First().Monitored == expectedArtistMonitored), false));
        }

        [TestCase(ImportListMonitorType.None, false)]
        [TestCase(ImportListMonitorType.SpecificAlbum, true)]
        [TestCase(ImportListMonitorType.EntireArtist, true)]
        public void should_add_if_not_existing_album(ImportListMonitorType monitor, bool expectedAlbumMonitored)
        {
            WithBookId();
            WithMonitorType(monitor);

            Subject.Execute(new ImportListSyncCommand());

            Mocker.GetMock<IAddAlbumService>()
                .Verify(v => v.AddAlbums(It.Is<List<Book>>(t => t.Count == 1 && t.First().Monitored == expectedAlbumMonitored), false));
        }

        [Test]
        public void should_not_add_artist_if_excluded_artist()
        {
            WithAuthorId();
            WithExcludedArtist();

            Subject.Execute(new ImportListSyncCommand());

            Mocker.GetMock<IAddArtistService>()
                .Verify(v => v.AddArtists(It.Is<List<Author>>(t => t.Count == 0), false));
        }

        [Test]
        public void should_not_add_album_if_excluded_album()
        {
            WithBookId();
            WithExcludedAlbum();

            Subject.Execute(new ImportListSyncCommand());

            Mocker.GetMock<IAddAlbumService>()
                .Verify(v => v.AddAlbums(It.Is<List<Book>>(t => t.Count == 0), false));
        }

        [Test]
        public void should_not_add_album_if_excluded_artist()
        {
            WithBookId();
            WithAuthorId();
            WithExcludedArtist();

            Subject.Execute(new ImportListSyncCommand());

            Mocker.GetMock<IAddAlbumService>()
                .Verify(v => v.AddAlbums(It.Is<List<Book>>(t => t.Count == 0), false));
        }
    }
}
