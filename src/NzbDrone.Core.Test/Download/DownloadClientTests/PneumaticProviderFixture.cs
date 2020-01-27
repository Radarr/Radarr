using System;
using System.IO;
using System.Net;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.Pneumatic;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Download.DownloadClientTests
{
    [TestFixture]
    public class PneumaticProviderFixture : CoreTest<Pneumatic>
    {
        private const string _nzbUrl = "http://www.nzbs.com/url";
        private const string _title = "30.Rock.S01E05.hdtv.xvid-LoL";
        private string _pneumaticFolder;
        private string _strmFolder;
        private string _nzbPath;
        private RemoteMovie _remoteMovie;

        [SetUp]
        public void Setup()
        {
            _pneumaticFolder = @"d:\nzb\pneumatic\".AsOsAgnostic();

            _nzbPath = Path.Combine(_pneumaticFolder, _title + ".nzb").AsOsAgnostic();
            _strmFolder = @"d:\unsorted tv\".AsOsAgnostic();

            _remoteMovie = new RemoteMovie();
            _remoteMovie.Release = new ReleaseInfo();
            _remoteMovie.Release.Title = _title;
            _remoteMovie.Release.DownloadUrl = _nzbUrl;

            _remoteMovie.ParsedMovieInfo = new ParsedMovieInfo();

            Subject.Definition = new DownloadClientDefinition();
            Subject.Definition.Settings = new PneumaticSettings
            {
                NzbFolder = _pneumaticFolder,
                StrmFolder = _strmFolder
            };
        }

        private void WithFailedDownload()
        {
            Mocker.GetMock<IHttpClient>().Setup(c => c.DownloadFile(It.IsAny<string>(), It.IsAny<string>())).Throws(new WebException());
        }

        [Test]
        public void should_download_file_if_it_doesnt_exist()
        {
            Subject.Download(_remoteMovie);

            Mocker.GetMock<IHttpClient>().Verify(c => c.DownloadFile(_nzbUrl, _nzbPath), Times.Once());
        }

        [Test]
        public void should_throw_on_failed_download()
        {
            WithFailedDownload();

            Assert.Throws<WebException>(() => Subject.Download(_remoteMovie));
        }

        [Test]
        public void should_throw_item_is_removed()
        {
            Assert.Throws<NotSupportedException>(() => Subject.RemoveItem("", true));
        }

        [Test]
        public void should_replace_illegal_characters_in_title()
        {
            var illegalTitle = "Saturday Night Live - S38E08 - Jeremy Renner/Maroon 5 [SDTV]";
            var expectedFilename = Path.Combine(_pneumaticFolder, "Saturday Night Live - S38E08 - Jeremy Renner+Maroon 5 [SDTV].nzb");
            _remoteMovie.Release.Title = illegalTitle;

            Subject.Download(_remoteMovie);

            Mocker.GetMock<IHttpClient>().Verify(c => c.DownloadFile(It.IsAny<string>(), expectedFilename), Times.Once());
        }
    }
}
