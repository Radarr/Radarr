using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Movies.Commands;
using NUnit.Framework;
using FluentAssertions;
using System.Data.SQLite;

namespace NzbDrone.Core.Test.Datastore.Converters
{
    [TestFixture]
    public class CommandConverterFixture : CoreTest<CommandConverter>
    {
        SQLiteParameter param;

        [SetUp]
        public void Setup()
        {
            param = new SQLiteParameter();
        }

        [Test]
        public void should_return_json_string_when_saving_boolean_to_db()
        {
            var command = new RefreshMovieCommand();

            Subject.SetValue(param, command);
            param.Value.Should().BeOfType<string>();
        }

        [Test]
        public void should_return_null_for_null_value_when_saving_to_db()
        {
            Subject.SetValue(param, null);
            param.Value.Should().BeNull();
        }

        [Test]
        public void should_return_command_when_getting_json_from_db()
        {
            var data = "{\"name\": \"RefreshMovie\"}";

            Subject.Parse(data).Should().BeOfType<RefreshMovieCommand>();
        }

        [Test]
        public void should_return_null_for_null_value_when_getting_from_db()
        {
            Subject.Parse(null).Should().BeNull();
        }
    }
}
