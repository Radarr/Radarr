using FluentAssertions;
using NUnit.Framework;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class NamingConfigFixture : IntegrationTest
    {

        [Test]
        public void should_be_able_to_get()
        {
            NamingConfig.GetSingle().Should().NotBeNull();
        }

        [Test]
        public void should_be_able_to_get_by_id()
        {
            var config = NamingConfig.GetSingle();
            NamingConfig.Get(config.Id).Should().NotBeNull();
            NamingConfig.Get(config.Id).Id.Should().Be(config.Id);
        }

        [Test]
        public void should_be_able_to_update()
        {
            var config = NamingConfig.GetSingle();
            config.RenameMovies = false;
            config.StandardMovieFormat = "{Movie Title}";

            var result = NamingConfig.Put(config);
            result.RenameMovies.Should().BeFalse();
            result.StandardMovieFormat.Should().Be(config.StandardMovieFormat);

        }

        [Test]
        public void should_get_bad_request_if_standard_format_is_empty()
        {
            IgnoreOnMonoVersions("5.12", "5.14");

            var config = NamingConfig.GetSingle();
            config.RenameMovies = true;
            config.StandardMovieFormat = "";

            var errors = NamingConfig.InvalidPut(config);
            errors.Should().NotBeNull();
        }

        [Test]
        public void should_get_bad_request_if_standard_format_doesnt_contain_title()
        {
            IgnoreOnMonoVersions("5.12", "5.14");

            var config = NamingConfig.GetSingle();
            config.RenameMovies = true;
            config.StandardMovieFormat = "{quality}";

            var errors = NamingConfig.InvalidPut(config);
            errors.Should().NotBeNull();
        }

        [Test]
        public void should_not_require_format_when_rename_episodes_is_false()
        {
            IgnoreOnMonoVersions("5.12", "5.14");

            var config = NamingConfig.GetSingle();
            config.RenameMovies = false;
            config.StandardMovieFormat = "";

            var errors = NamingConfig.InvalidPut(config);
            errors.Should().NotBeNull();
        }

        [Test]
        public void should_require_format_when_rename_episodes_is_true()
        {
            IgnoreOnMonoVersions("5.12", "5.14");

            var config = NamingConfig.GetSingle();
            config.RenameMovies = true;
            config.StandardMovieFormat = "";

            var errors = NamingConfig.InvalidPut(config);
            errors.Should().NotBeNull();
        }

        [Test]
        public void should_get_bad_request_if_movie_folder_format_does_not_contain_movie_title()
        {
            IgnoreOnMonoVersions("5.12", "5.14");

            var config = NamingConfig.GetSingle();
            config.RenameMovies = true;
            config.MovieFolderFormat = "This and That";

            var errors = NamingConfig.InvalidPut(config);
            errors.Should().NotBeNull();
        }
    }
}
