using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using System.Collections.Generic;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Test.Datastore
{
    [TestFixture]
    public class DatabaseRelationshipFixture : DbTest
    {
        [SetUp]
        public void Setup()
        {
            // This is kinda hacky here, since we are kinda testing if the QualityDef converter works as well.
        }

        [Test]
        public void embedded_document_as_json()
        {
            var quality = new QualityModel { Quality = Quality.Bluray720p, Revision = new Revision(version: 2 )};
            var languages = new List<Language> { Language.English };

            var history = Builder<History.History>.CreateNew()
                            .With(c => c.Id = 0)
                            .With(c => c.Quality = quality)
                            .With(c => c.Languages = languages)
                            .Build();

            Db.Insert(history);

            var loadedQuality = Db.Single<History.History>().Quality;
            loadedQuality.Should().Be(quality);
        }

        [Test]
        public void embedded_list_of_document_with_json()
        {
            var languages = new List<Language> { Language.English };

            var history = Builder<History.History>.CreateListOfSize(2)
                            .All().With(c => c.Id = 0)
                            .With(c => c.Languages = languages)
                            .Build().ToList();

            history[0].Quality = new QualityModel { Quality = Quality.HDTV1080p, Revision = new Revision(version: 2)};
            history[1].Quality = new QualityModel { Quality = Quality.Bluray720p, Revision = new Revision(version: 2)};



            Db.InsertMany(history);

            var returnedHistory = Db.All<History.History>();

            returnedHistory[0].Quality.Quality.Should().Be(Quality.HDTV1080p);
        }
    }
}
