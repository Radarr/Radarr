using System.Collections.Generic;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.History;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles
{
    [TestFixture]
    public class UpdateMovieFileQualityServiceFixture : CoreTest<UpdateMovieFileQualityService>
    {
        private MovieFile _movieFile;
        private QualityModel _oldQuality;
        private QualityModel _newQuality;

        private ParsedMovieInfo _newInfo;

        [SetUp]
        public void Setup()
        {
            _movieFile = Builder<MovieFile>.CreateNew().With(m => m.MovieId = 0).Build();

            _oldQuality = new QualityModel(Quality.Bluray720p);

            _movieFile.Quality = _oldQuality;

            _newQuality = _oldQuality.JsonClone();
            var format = new CustomFormats.CustomFormat("Awesome Format");
            format.Id = 1;
            _newQuality.CustomFormats = new List<CustomFormats.CustomFormat>{format};

            _newInfo = new ParsedMovieInfo
            {
                Quality = _newQuality
            };

            Mocker.GetMock<IMediaFileService>().Setup(s => s.GetMovies(It.IsAny<IEnumerable<int>>()))
                .Returns(new List<MovieFile>{_movieFile});

            Mocker.GetMock<IHistoryService>().Setup(s => s.GetByMovieId(It.IsAny<int>(), null))
                .Returns(new List<History.History>());
        }

        private void ExecuteCommand()
        {
            Subject.Execute(new UpdateMovieFileQualityCommand(new List<int>{0}));
        }

        [Test]
        public void should_not_update_if_unable_to_parse()
        {
            ExecuteCommand();

            ExceptionVerification.ExpectedWarns(1);
            
            Mocker.GetMock<IMediaFileService>().Verify(s => s.Update(It.IsAny<MovieFile>()), Times.Never());
        }

        [Test]
        public void should_update_with_new_formats()
        {
            Mocker.GetMock<IParsingService>().Setup(s => s.ParseMovieInfo(It.IsAny<string>(), It.IsAny<List<object>>()))
                .Returns(_newInfo);

            ExecuteCommand();

            Mocker.GetMock<IMediaFileService>().Verify(s => s.Update(It.Is<MovieFile>(f => f.Quality.CustomFormats == _newQuality.CustomFormats)), Times.Once());
        }

        [Test]
        public void should_use_imported_history_title()
        {
            var imported = Builder<History.History>.CreateNew()
                .With(h => h.EventType = HistoryEventType.DownloadFolderImported)
                .With(h => h.SourceTitle = "My Movie 2018.mkv").Build();
            Mocker.GetMock<IHistoryService>().Setup(s => s.GetByMovieId(It.IsAny<int>(), null))
                .Returns(new List<History.History> {imported});

            ExecuteCommand();

            Mocker.GetMock<IParsingService>().Verify(s => s.ParseMovieInfo("My Movie 2018.mkv", It.IsAny<List<object>>()));
        }
    }
}
