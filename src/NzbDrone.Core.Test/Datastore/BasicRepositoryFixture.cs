using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore
{

    [TestFixture]
    public class
        BasicRepositoryFixture : DbTest<BasicRepository<ScheduledTask>, ScheduledTask>
    {
        private ScheduledTask _basicType;
        private List<ScheduledTask> _basicList;

        [SetUp]
        public void Setup()
        {
            _basicType = Builder<ScheduledTask>
                    .CreateNew()
                    .With(c => c.Id = 0)
                    .With(c => c.LastExecution = DateTime.UtcNow)
                    .Build();

            _basicList = Builder<ScheduledTask>
                .CreateListOfSize(5)
                .All()
                .With(x => x.Id = 0)
                .BuildList();
        }

        [Test]
        public void should_be_able_to_insert()
        {
            Subject.Insert(_basicType);
            Subject.All().Should().HaveCount(1);
        }

        [Test]
        public void should_be_able_to_insert_many()
        {
            Subject.InsertMany(_basicList);
            Subject.All().Should().HaveCount(5);
        }

        [Test]
        public void purge_should_delete_all()
        {
            Subject.InsertMany(Builder<ScheduledTask>.CreateListOfSize(10).BuildListOfNew());

            AllStoredModels.Should().HaveCount(10);

            Subject.Purge();

            AllStoredModels.Should().BeEmpty();

        }

        [Test]
        public void should_be_able_to_delete_model()
        {
            Subject.Insert(_basicType);
            Subject.All().Should().HaveCount(1);

            Subject.Delete(_basicType.Id);
            Subject.All().Should().BeEmpty();
        }

        [Test]
        public void should_be_able_to_delete_many()
        {
            Subject.InsertMany(_basicList);
            Subject.All().Should().HaveCount(5);

            Subject.DeleteMany(_basicList.Take(2).ToList());
            Subject.All().Should().HaveCount(3);
        }

        [Test]
        public void should_be_able_to_find_by_id()
        {
            Subject.Insert(_basicType);
            var storeObject = Subject.Get(_basicType.Id);

            storeObject.Should().BeEquivalentTo(_basicType, o=>o.IncludingAllRuntimeProperties());
        }

        [Test]
        public void should_be_able_to_find_by_multiple_id()
        {
            Subject.InsertMany(_basicList);
            var storeObject = Subject.Get(_basicList.Take(2).Select(x => x.Id));
            storeObject.Should().HaveCount(2);
        }

        [Test]
        public void should_be_able_to_update()
        {
            Subject.Insert(_basicType);
            _basicType.Interval = 999;

            Subject.Update(_basicType);

            Subject.All().First().Interval.Should().Be(999);
        }

        [Test]
        public void should_be_able_to_update_many()
        {
            Subject.InsertMany(_basicList);
            _basicList.ForEach(x => x.Interval = 999);

            Subject.UpdateMany(_basicList);

            Subject.All().All(x => x.Interval == 999);
        }

        [Test]
        public void should_be_able_to_update_single_field()
        {
            Subject.Insert(_basicType);
            _basicType.Interval = 999;
            _basicType.LastExecution = DateTime.UtcNow;

            Subject.SetFields(_basicType, x => x.Interval);

            var dbValue = Subject.Single();
            dbValue.Interval.Should().Be(999);
            dbValue.LastExecution.Should().NotBe(_basicType.LastExecution);
        }

        [Test]
        public void should_be_able_to_update_many_single_field()
        {
            Subject.InsertMany(_basicList);
            _basicList.ForEach(x => x.Interval = 999);
            _basicList.ForEach(x => x.LastExecution = DateTime.UtcNow);

            Subject.SetFields(_basicList, x => x.Interval);

            var dbValue = Subject.All().First();
            dbValue.Interval.Should().Be(999);
            dbValue.LastExecution.Should().NotBe(_basicType.LastExecution);
        }

        [Test]
        public void should_be_able_to_get_single()
        {
            Subject.Insert(_basicType);
            Subject.SingleOrDefault().Should().NotBeNull();
        }

        [Test]
        public void single_or_default_on_empty_table_should_return_null()
        {
            Subject.SingleOrDefault().Should().BeNull();
        }

        [Test]
        public void getting_model_with_invalid_id_should_throw()
        {
            Assert.Throws<ModelNotFoundException>(() => Subject.Get(12));
        }

        [Test]
        public void get_all_with_empty_db_should_return_empty_list()
        {
            Subject.All().Should().BeEmpty();
        }


        [Test]
        public void should_be_able_to_call_ToList_on_empty_quariable()
        {
            Subject.All().ToList().Should().BeEmpty();
        }
    }
}
