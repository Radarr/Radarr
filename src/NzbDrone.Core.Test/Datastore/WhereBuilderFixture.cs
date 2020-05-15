using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore
{
    [TestFixture]
    public class WhereBuilderFixture : CoreTest
    {
        private WhereBuilder _subject;

        [OneTimeSetUp]
        public void MapTables()
        {
            // Generate table mapping
            Mocker.Resolve<DbFactory>();
        }

        private WhereBuilder Where(Expression<Func<Movie, bool>> filter)
        {
            return new WhereBuilder(filter, true);
        }

        [Test]
        public void where_equal_const()
        {
            _subject = Where(x => x.Id == 10);

            var name = _subject.Parameters.ParameterNames.First();
            _subject.ToString().Should().Be($"(\"Movies\".\"Id\" = @{name})");
            _subject.Parameters.Get<int>(name).Should().Be(10);
        }

        [Test]
        public void where_equal_variable()
        {
            var id = 10;
            _subject = Where(x => x.Id == id);

            var name = _subject.Parameters.ParameterNames.First();
            _subject.ToString().Should().Be($"(\"Movies\".\"Id\" = @{name})");
            _subject.Parameters.Get<int>(name).Should().Be(id);
        }

        [Test]
        public void where_throws_without_concrete_condition_if_requiresConcreteCondition()
        {
            var movie = new Movie();
            Expression<Func<Movie, bool>> filter = (x) => x.Id == movie.Id;
            _subject = new WhereBuilder(filter, true);
            Assert.Throws<InvalidOperationException>(() => _subject.ToString());
        }

        [Test]
        public void where_allows_abstract_condition_if_not_requiresConcreteCondition()
        {
            var movie = new Movie();
            Expression<Func<Movie, bool>> filter = (x) => x.Id == movie.Id;
            _subject = new WhereBuilder(filter, false);
            _subject.ToString().Should().Be($"(\"Movies\".\"Id\" = \"Movies\".\"Id\")");
        }

        [Test]
        public void where_string_is_null()
        {
            _subject = Where(x => x.ImdbId == null);

            _subject.ToString().Should().Be($"(\"Movies\".\"ImdbId\" IS NULL)");
        }

        [Test]
        public void where_string_is_null_value()
        {
            string imdb = null;
            _subject = Where(x => x.ImdbId == imdb);

            _subject.ToString().Should().Be($"(\"Movies\".\"ImdbId\" IS NULL)");
        }

        [Test]
        public void where_column_contains_string()
        {
            var test = "small";
            _subject = Where(x => x.CleanTitle.Contains(test));

            var name = _subject.Parameters.ParameterNames.First();
            _subject.ToString().Should().Be($"(\"Movies\".\"CleanTitle\" LIKE '%' || @{name} || '%')");
            _subject.Parameters.Get<string>(name).Should().Be(test);
        }

        [Test]
        public void where_string_contains_column()
        {
            var test = "small";
            _subject = Where(x => test.Contains(x.CleanTitle));

            var name = _subject.Parameters.ParameterNames.First();
            _subject.ToString().Should().Be($"(@{name} LIKE '%' || \"Movies\".\"CleanTitle\" || '%')");
            _subject.Parameters.Get<string>(name).Should().Be(test);
        }

        [Test]
        public void where_column_starts_with_string()
        {
            var test = "small";
            _subject = Where(x => x.CleanTitle.StartsWith(test));

            var name = _subject.Parameters.ParameterNames.First();
            _subject.ToString().Should().Be($"(\"Movies\".\"CleanTitle\" LIKE @{name} || '%')");
            _subject.Parameters.Get<string>(name).Should().Be(test);
        }

        [Test]
        public void where_column_ends_with_string()
        {
            var test = "small";
            _subject = Where(x => x.CleanTitle.EndsWith(test));

            var name = _subject.Parameters.ParameterNames.First();
            _subject.ToString().Should().Be($"(\"Movies\".\"CleanTitle\" LIKE '%' || @{name})");
            _subject.Parameters.Get<string>(name).Should().Be(test);
        }

        [Test]
        public void where_in_list()
        {
            var list = new List<int> { 1, 2, 3 };
            _subject = Where(x => list.Contains(x.Id));

            var name = _subject.Parameters.ParameterNames.First();
            _subject.ToString().Should().Be($"(\"Movies\".\"Id\" IN @{name})");

            var param = _subject.Parameters.Get<List<int>>(name);
            param.Should().BeEquivalentTo(list);
        }

        [Test]
        public void where_in_list_2()
        {
            var list = new List<int> { 1, 2, 3 };
            _subject = Where(x => x.CleanTitle == "test" && list.Contains(x.Id));

            var names = _subject.Parameters.ParameterNames.ToList();
            _subject.ToString().Should().Be($"((\"Movies\".\"CleanTitle\" = @{names[0]}) AND (\"Movies\".\"Id\" IN @{names[1]}))");
        }

        [Test]
        public void enum_as_int()
        {
            _subject = Where(x => x.Status == MovieStatusType.Released);

            var name = _subject.Parameters.ParameterNames.First();
            _subject.ToString().Should().Be($"(\"Movies\".\"Status\" = @{name})");
        }

        [Test]
        public void enum_in_list()
        {
            var allowed = new List<MovieStatusType> { MovieStatusType.InCinemas, MovieStatusType.Released };
            _subject = Where(x => allowed.Contains(x.Status));

            var name = _subject.Parameters.ParameterNames.First();
            _subject.ToString().Should().Be($"(\"Movies\".\"Status\" IN @{name})");
        }

        [Test]
        public void enum_in_array()
        {
            var allowed = new MovieStatusType[] { MovieStatusType.InCinemas, MovieStatusType.Released };
            _subject = Where(x => allowed.Contains(x.Status));

            var name = _subject.Parameters.ParameterNames.First();
            _subject.ToString().Should().Be($"(\"Movies\".\"Status\" IN @{name})");
        }
    }
}
