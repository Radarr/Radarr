using FizzWare.NBuilder;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Test.Qualities;

namespace NzbDrone.Core.Test.Datastore
{

    [TestFixture]
    public class MarrDataLazyLoadingFixture : DbTest
    {
        [SetUp]
        public void Setup()
        {
            QualityDefinitionServiceFixture.SetupDefaultDefinitions();

            var profile = new Profile
                {
                    Name = "Test",
                    Cutoff = QualityWrapper.Dynamic.WEBDL720p,
                    Items = Qualities.QualityFixture.GetDefaultQualities()
                };


            profile = Db.Insert(profile);

            var series = Builder<Movie>.CreateListOfSize(1)
                .All()
                .With(v => v.ProfileId = profile.Id)
                .BuildListOfNew();

            Db.InsertMany(series);

            var episodeFiles = Builder<MovieFile>.CreateListOfSize(1)
                .All()
                .With(v => v.MovieId = series[0].Id)
                .With(v => v.Quality = new QualityModel())
                .BuildListOfNew();

            Db.InsertMany(episodeFiles);

            var episodes = Builder<Movie>.CreateListOfSize(10)
                .All()
                .With(v => v.Monitored = true)
                .With(v => v.MovieFileId = episodeFiles[0].Id)
                .BuildListOfNew();

            Db.InsertMany(episodes);
        }


    }
}
