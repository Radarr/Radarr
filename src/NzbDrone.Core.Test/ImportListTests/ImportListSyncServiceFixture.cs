using System.Linq;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Music;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.ImportLists.Exclusions;

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

            _importListReports = new List<ImportListItemInfo>{importListItem1};

            Mocker.GetMock<IFetchAndParseImportList>()
                .Setup(v => v.Fetch())
                .Returns(_importListReports);

            Mocker.GetMock<ISearchForNewArtist>()
                .Setup(v => v.SearchForNewArtist(It.IsAny<string>()))
                .Returns(new List<Artist>());

            Mocker.GetMock<ISearchForNewAlbum>()
                .Setup(v => v.SearchForNewAlbum(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new List<Album>());

            Mocker.GetMock<IImportListFactory>()
                .Setup(v => v.Get(It.IsAny<int>()))
                .Returns(new ImportListDefinition{ ShouldMonitor = ImportListMonitorType.SpecificAlbum });

            Mocker.GetMock<IFetchAndParseImportList>()
                .Setup(v => v.Fetch())
                .Returns(_importListReports);

            Mocker.GetMock<IImportListExclusionService>()
                .Setup(v => v.All())
                .Returns(new List<ImportListExclusion>());
        }

        private void WithAlbum()
        {
            _importListReports.First().Album = "Meteora";
        }

        private void WithArtistId()
        {
            _importListReports.First().ArtistMusicBrainzId = "f59c5520-5f46-4d2c-b2c4-822eabf53419";
        }

        private void WithAlbumId()
        {
            _importListReports.First().AlbumMusicBrainzId = "09474d62-17dd-3a4f-98fb-04c65f38a479";
        }

        private void WithExistingArtist()
        {
            Mocker.GetMock<IArtistService>()
                .Setup(v => v.FindById(_importListReports.First().ArtistMusicBrainzId))
                .Returns(new Artist{ForeignArtistId = _importListReports.First().ArtistMusicBrainzId });
        }

        private void WithExcludedArtist()
        {
            Mocker.GetMock<IImportListExclusionService>()
                .Setup(v => v.All())
                .Returns(new List<ImportListExclusion> {
                    new ImportListExclusion {
                        ForeignId = "f59c5520-5f46-4d2c-b2c4-822eabf53419"
                    }
                });
        }

        private void WithMonitorType(ImportListMonitorType monitor)
        {
            Mocker.GetMock<IImportListFactory>()
                .Setup(v => v.Get(It.IsAny<int>()))
                .Returns(new ImportListDefinition{ ShouldMonitor = monitor });
        }

        [Test]
        public void should_search_if_artist_title_and_no_artist_id()
        {
            Subject.Execute(new ImportListSyncCommand());

            Mocker.GetMock<ISearchForNewArtist>()
                .Verify(v => v.SearchForNewArtist(It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_not_search_if_artist_title_and_artist_id()
        {
            WithArtistId();
            Subject.Execute(new ImportListSyncCommand());

            Mocker.GetMock<ISearchForNewArtist>()
                .Verify(v => v.SearchForNewArtist(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_search_if_album_title_and_no_album_id()
        {
            WithAlbum();
            Subject.Execute(new ImportListSyncCommand());

            Mocker.GetMock<ISearchForNewAlbum>()
                .Verify(v => v.SearchForNewAlbum(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_not_search_if_album_title_and_album_id()
        {
            WithAlbum();
            WithAlbumId();
            Subject.Execute(new ImportListSyncCommand());

            Mocker.GetMock<ISearchForNewAlbum>()
                .Verify(v => v.SearchForNewAlbum(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_not_search_if_all_info()
        {
            WithArtistId();
            WithAlbum();
            WithAlbumId();
            Subject.Execute(new ImportListSyncCommand());

            Mocker.GetMock<ISearchForNewArtist>()
                .Verify(v => v.SearchForNewArtist(It.IsAny<string>()), Times.Never());

            Mocker.GetMock<ISearchForNewAlbum>()
                .Verify(v => v.SearchForNewAlbum(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_not_add_if_existing_artist()
        {
            WithArtistId();
            WithAlbum();
            WithAlbumId();
            WithExistingArtist();

            Subject.Execute(new ImportListSyncCommand());

            Mocker.GetMock<IAddArtistService>()
                .Verify(v => v.AddArtists(It.Is<List<Artist>>(t=>t.Count == 0)));
        }

        [TestCase(ImportListMonitorType.None, false)]
        [TestCase(ImportListMonitorType.SpecificAlbum, true)]
        [TestCase(ImportListMonitorType.EntireArtist, true)]
        public void should_add_if_not_existing_artist(ImportListMonitorType monitor, bool expectedArtistMonitored)
        {
            WithArtistId();
            WithAlbum();
            WithAlbumId();
            WithMonitorType(monitor);

            Subject.Execute(new ImportListSyncCommand());

            Mocker.GetMock<IAddArtistService>()
                .Verify(v => v.AddArtists(It.Is<List<Artist>>(t => t.Count == 1 && t.First().Monitored == expectedArtistMonitored)));
        }

        [Test]
        public void should_not_add_if_excluded_artist()
        {
            WithArtistId();
            WithAlbum();
            WithAlbumId();
            WithExcludedArtist();

            Subject.Execute(new ImportListSyncCommand());

            Mocker.GetMock<IAddArtistService>()
                .Verify(v => v.AddArtists(It.Is<List<Artist>>(t => t.Count == 0)));
        }

        [Test]
        public void should_mark_album_for_monitor_if_album_id_and_specific_monitor_selected()
        {
            WithArtistId();
            WithAlbum();
            WithAlbumId();
            WithMonitorType(ImportListMonitorType.SpecificAlbum);

            Subject.Execute(new ImportListSyncCommand());

            Mocker.GetMock<IAddArtistService>()
                .Verify(v => v.AddArtists(It.Is<List<Artist>>(t => t.Count == 1 && t.First().AddOptions.AlbumsToMonitor.Contains("09474d62-17dd-3a4f-98fb-04c65f38a479"))));
        }

        [Test]
        public void should_not_mark_album_for_monitor_if_album_id_and_monitor_all_selected()
        {
            WithArtistId();
            WithAlbum();
            WithAlbumId();
            WithMonitorType(ImportListMonitorType.EntireArtist);

            Subject.Execute(new ImportListSyncCommand());

            Mocker.GetMock<IAddArtistService>()
                .Verify(v => v.AddArtists(It.Is<List<Artist>>(t => t.Count == 1 && !t.First().AddOptions.AlbumsToMonitor.Any())));
        }

        [Test]
        public void should_not_mark_album_for_monitor_if_no_album_id()
        {
            WithArtistId();

            Subject.Execute(new ImportListSyncCommand());

            Mocker.GetMock<IAddArtistService>()
                .Verify(v => v.AddArtists(It.Is<List<Artist>>(t => t.Count == 1 && t.First().AddOptions.AlbumsToMonitor.Count == 0)));
        }
    }
}
