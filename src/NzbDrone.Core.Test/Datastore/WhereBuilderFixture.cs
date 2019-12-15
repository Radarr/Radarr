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
        private WhereBuilder Subject;

        [OneTimeSetUp]
        public void MapTables()
        {
            // Generate table mapping
            Mocker.Resolve<DbFactory>();
        }

        private WhereBuilder Where(Expression<Func<Movie, bool>> filter)
        {
            return new WhereBuilder(filter);
        }

        [Test]
        public void where_equal_const()
        {
            Subject = Where(x => x.Id == 10);

            var name = Subject.Parameters.ParameterNames.First();
            Subject.ToString().Should().Be($"(\"Movies\".\"Id\" = @{name})");
            Subject.Parameters.Get<int>(name).Should().Be(10);
        }

        [Test]
        public void where_equal_variable()
        {
            var id = 10;
            Subject = Where(x => x.Id == id);

            var name = Subject.Parameters.ParameterNames.First();
            Subject.ToString().Should().Be($"(\"Movies\".\"Id\" = @{name})");
            Subject.Parameters.Get<int>(name).Should().Be(id);
        }

        [Test]
        public void where_column_contains_string()
        {
            var test = "small";
            Subject = Where(x => x.CleanTitle.Contains(test));

            var name = Subject.Parameters.ParameterNames.First();
            Subject.ToString().Should().Be($"(\"Movies\".\"CleanTitle\" LIKE '%' || @{name} || '%')");
            Subject.Parameters.Get<string>(name).Should().Be(test);
        }

        [Test]
        public void where_string_contains_column()
        {
            var test = "small";
            Subject = Where(x => test.Contains(x.CleanTitle));

            var name = Subject.Parameters.ParameterNames.First();
            Subject.ToString().Should().Be($"(@{name} LIKE '%' || \"Movies\".\"CleanTitle\" || '%')");
            Subject.Parameters.Get<string>(name).Should().Be(test);
        }

        [Test]
        public void where_in_list()
        {
            var list = new List<int> {1, 2, 3};
            Subject = Where(x => list.Contains(x.Id));

            var name = Subject.Parameters.ParameterNames.First();
            Subject.ToString().Should().Be($"(\"Movies\".\"Id\" IN @{name})");

            var param = Subject.Parameters.Get<List<int>>(name);
            param.Should().BeEquivalentTo(list);
        }

        [Test]
        public void where_in_list_2()
        {
            var list = new List<int> {1, 2, 3};
            Subject = Where(x => x.CleanTitle == "test" && list.Contains(x.Id));

            var names = Subject.Parameters.ParameterNames.ToList();
            Subject.ToString().Should().Be($"((\"Movies\".\"CleanTitle\" = @{names[0]}) AND (\"Movies\".\"Id\" IN @{names[1]}))");
        }

        [Test]
        public void enum_as_int()
        {
            Subject = Where(x => x.PathState == MoviePathState.Static);

            var name = Subject.Parameters.ParameterNames.First();
            Subject.ToString().Should().Be($"(\"Movies\".\"PathState\" = @{name})");
        }

        [Test]
        public void enum_in_list()
        {
            var allowed = new List<MoviePathState> { MoviePathState.Dynamic, MoviePathState.Static };
            Subject = Where(x => allowed.Contains(x.PathState));

            var name = Subject.Parameters.ParameterNames.First();
            Subject.ToString().Should().Be($"(\"Movies\".\"PathState\" IN @{name})");
        }

        [Test]
        public void enum_in_array()
        {
            var allowed = new MoviePathState[] { MoviePathState.Dynamic, MoviePathState.Static };
            Subject = Where(x => allowed.Contains(x.PathState));

            var name = Subject.Parameters.ParameterNames.First();
            Subject.ToString().Should().Be($"(\"Movies\".\"PathState\" IN @{name})");
        }
    }
}
