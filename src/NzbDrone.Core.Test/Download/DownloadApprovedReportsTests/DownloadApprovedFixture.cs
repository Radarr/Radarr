using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Download.DownloadApprovedReportsTests
{
    [TestFixture]
    public class DownloadApprovedFixture : CoreTest<ProcessDownloadDecisions>
    {
        [SetUp]
        public void SetUp()
        {
            Mocker.GetMock<IPrioritizeDownloadDecision>()
                .Setup(v => v.PrioritizeDecisions(It.IsAny<List<DownloadDecision>>()))
                .Returns<List<DownloadDecision>>(v => v);
        }

        private Book GetAlbum(int id)
        {
            return Builder<Book>.CreateNew()
                            .With(e => e.Id = id)
                            .Build();
        }

        private RemoteBook GetRemoteAlbum(List<Book> albums, QualityModel quality, DownloadProtocol downloadProtocol = DownloadProtocol.Usenet)
        {
            var remoteAlbum = new RemoteBook();
            remoteAlbum.ParsedBookInfo = new ParsedBookInfo();
            remoteAlbum.ParsedBookInfo.Quality = quality;

            remoteAlbum.Books = new List<Book>();
            remoteAlbum.Books.AddRange(albums);

            remoteAlbum.Release = new ReleaseInfo();
            remoteAlbum.Release.DownloadProtocol = downloadProtocol;
            remoteAlbum.Release.PublishDate = DateTime.UtcNow;

            remoteAlbum.Author = Builder<Author>.CreateNew()
                .With(e => e.QualityProfile = new QualityProfile { Items = Qualities.QualityFixture.GetDefaultQualities() })
                .Build();

            return remoteAlbum;
        }

        [Test]
        public void should_download_report_if_album_was_not_already_downloaded()
        {
            var albums = new List<Book> { GetAlbum(1) };
            var remoteAlbum = GetRemoteAlbum(albums, new QualityModel(Quality.MP3_320));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteAlbum));

            Subject.ProcessDecisions(decisions);
            Mocker.GetMock<IDownloadService>().Verify(v => v.DownloadReport(It.IsAny<RemoteBook>()), Times.Once());
        }

        [Test]
        public void should_only_download_album_once()
        {
            var albums = new List<Book> { GetAlbum(1) };
            var remoteAlbum = GetRemoteAlbum(albums, new QualityModel(Quality.MP3_320));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteAlbum));
            decisions.Add(new DownloadDecision(remoteAlbum));

            Subject.ProcessDecisions(decisions);
            Mocker.GetMock<IDownloadService>().Verify(v => v.DownloadReport(It.IsAny<RemoteBook>()), Times.Once());
        }

        [Test]
        public void should_not_download_if_any_album_was_already_downloaded()
        {
            var remoteAlbum1 = GetRemoteAlbum(
                                                    new List<Book> { GetAlbum(1) },
                                                    new QualityModel(Quality.MP3_320));

            var remoteAlbum2 = GetRemoteAlbum(
                                                    new List<Book> { GetAlbum(1), GetAlbum(2) },
                                                    new QualityModel(Quality.MP3_320));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteAlbum1));
            decisions.Add(new DownloadDecision(remoteAlbum2));

            Subject.ProcessDecisions(decisions);
            Mocker.GetMock<IDownloadService>().Verify(v => v.DownloadReport(It.IsAny<RemoteBook>()), Times.Once());
        }

        [Test]
        public void should_return_downloaded_reports()
        {
            var albums = new List<Book> { GetAlbum(1) };
            var remoteAlbum = GetRemoteAlbum(albums, new QualityModel(Quality.MP3_320));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteAlbum));

            Subject.ProcessDecisions(decisions).Grabbed.Should().HaveCount(1);
        }

        [Test]
        public void should_return_all_downloaded_reports()
        {
            var remoteAlbum1 = GetRemoteAlbum(
                                                    new List<Book> { GetAlbum(1) },
                                                    new QualityModel(Quality.MP3_320));

            var remoteAlbum2 = GetRemoteAlbum(
                                                    new List<Book> { GetAlbum(2) },
                                                    new QualityModel(Quality.MP3_320));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteAlbum1));
            decisions.Add(new DownloadDecision(remoteAlbum2));

            Subject.ProcessDecisions(decisions).Grabbed.Should().HaveCount(2);
        }

        [Test]
        public void should_only_return_downloaded_reports()
        {
            var remoteAlbum1 = GetRemoteAlbum(
                                                    new List<Book> { GetAlbum(1) },
                                                    new QualityModel(Quality.MP3_320));

            var remoteAlbum2 = GetRemoteAlbum(
                                                    new List<Book> { GetAlbum(2) },
                                                    new QualityModel(Quality.MP3_320));

            var remoteAlbum3 = GetRemoteAlbum(
                                                    new List<Book> { GetAlbum(2) },
                                                    new QualityModel(Quality.MP3_320));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteAlbum1));
            decisions.Add(new DownloadDecision(remoteAlbum2));
            decisions.Add(new DownloadDecision(remoteAlbum3));

            Subject.ProcessDecisions(decisions).Grabbed.Should().HaveCount(2);
        }

        [Test]
        public void should_not_add_to_downloaded_list_when_download_fails()
        {
            var albums = new List<Book> { GetAlbum(1) };
            var remoteAlbum = GetRemoteAlbum(albums, new QualityModel(Quality.MP3_320));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteAlbum));

            Mocker.GetMock<IDownloadService>().Setup(s => s.DownloadReport(It.IsAny<RemoteBook>())).Throws(new Exception());
            Subject.ProcessDecisions(decisions).Grabbed.Should().BeEmpty();
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_return_an_empty_list_when_none_are_appproved()
        {
            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(new RemoteBook(), new Rejection("Failure!")));
            decisions.Add(new DownloadDecision(new RemoteBook(), new Rejection("Failure!")));

            Subject.GetQualifiedReports(decisions).Should().BeEmpty();
        }

        [Test]
        public void should_not_grab_if_pending()
        {
            var albums = new List<Book> { GetAlbum(1) };
            var remoteAlbum = GetRemoteAlbum(albums, new QualityModel(Quality.MP3_320));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteAlbum, new Rejection("Failure!", RejectionType.Temporary)));

            Subject.ProcessDecisions(decisions);
            Mocker.GetMock<IDownloadService>().Verify(v => v.DownloadReport(It.IsAny<RemoteBook>()), Times.Never());
        }

        [Test]
        public void should_not_add_to_pending_if_album_was_grabbed()
        {
            var albums = new List<Book> { GetAlbum(1) };
            var remoteAlbum = GetRemoteAlbum(albums, new QualityModel(Quality.MP3_320));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteAlbum));
            decisions.Add(new DownloadDecision(remoteAlbum, new Rejection("Failure!", RejectionType.Temporary)));

            Subject.ProcessDecisions(decisions);
            Mocker.GetMock<IPendingReleaseService>().Verify(v => v.AddMany(It.IsAny<List<Tuple<DownloadDecision, PendingReleaseReason>>>()), Times.Never());
        }

        [Test]
        public void should_add_to_pending_even_if_already_added_to_pending()
        {
            var albums = new List<Book> { GetAlbum(1) };
            var remoteAlbum = GetRemoteAlbum(albums, new QualityModel(Quality.MP3_320));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteAlbum, new Rejection("Failure!", RejectionType.Temporary)));
            decisions.Add(new DownloadDecision(remoteAlbum, new Rejection("Failure!", RejectionType.Temporary)));

            Subject.ProcessDecisions(decisions);
            Mocker.GetMock<IPendingReleaseService>().Verify(v => v.AddMany(It.IsAny<List<Tuple<DownloadDecision, PendingReleaseReason>>>()), Times.Once());
        }

        [Test]
        public void should_add_to_failed_if_already_failed_for_that_protocol()
        {
            var albums = new List<Book> { GetAlbum(1) };
            var remoteAlbum = GetRemoteAlbum(albums, new QualityModel(Quality.MP3_320));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteAlbum));
            decisions.Add(new DownloadDecision(remoteAlbum));

            Mocker.GetMock<IDownloadService>().Setup(s => s.DownloadReport(It.IsAny<RemoteBook>()))
                  .Throws(new DownloadClientUnavailableException("Download client failed"));

            Subject.ProcessDecisions(decisions);
            Mocker.GetMock<IDownloadService>().Verify(v => v.DownloadReport(It.IsAny<RemoteBook>()), Times.Once());
        }

        [Test]
        public void should_not_add_to_failed_if_failed_for_a_different_protocol()
        {
            var albums = new List<Book> { GetAlbum(1) };
            var remoteAlbum = GetRemoteAlbum(albums, new QualityModel(Quality.MP3_320), DownloadProtocol.Usenet);
            var remoteAlbum2 = GetRemoteAlbum(albums, new QualityModel(Quality.MP3_320), DownloadProtocol.Torrent);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteAlbum));
            decisions.Add(new DownloadDecision(remoteAlbum2));

            Mocker.GetMock<IDownloadService>().Setup(s => s.DownloadReport(It.Is<RemoteBook>(r => r.Release.DownloadProtocol == DownloadProtocol.Usenet)))
                  .Throws(new DownloadClientUnavailableException("Download client failed"));

            Subject.ProcessDecisions(decisions);
            Mocker.GetMock<IDownloadService>().Verify(v => v.DownloadReport(It.Is<RemoteBook>(r => r.Release.DownloadProtocol == DownloadProtocol.Usenet)), Times.Once());
            Mocker.GetMock<IDownloadService>().Verify(v => v.DownloadReport(It.Is<RemoteBook>(r => r.Release.DownloadProtocol == DownloadProtocol.Torrent)), Times.Once());
        }

        [Test]
        public void should_add_to_rejected_if_release_unavailable_on_indexer()
        {
            var albums = new List<Book> { GetAlbum(1) };
            var remoteAlbum = GetRemoteAlbum(albums, new QualityModel(Quality.MP3_320));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteAlbum));

            Mocker.GetMock<IDownloadService>()
                  .Setup(s => s.DownloadReport(It.IsAny<RemoteBook>()))
                  .Throws(new ReleaseUnavailableException(remoteAlbum.Release, "That 404 Error is not just a Quirk"));

            var result = Subject.ProcessDecisions(decisions);

            result.Grabbed.Should().BeEmpty();
            result.Rejected.Should().NotBeEmpty();

            ExceptionVerification.ExpectedWarns(1);
        }
    }
}
