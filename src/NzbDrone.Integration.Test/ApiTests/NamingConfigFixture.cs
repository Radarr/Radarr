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
            config.RenameBooks = false;
            config.StandardBookFormat = "{Author Name} - {Book Title}";

            var result = NamingConfig.Put(config);
            result.RenameBooks.Should().BeFalse();
            result.StandardBookFormat.Should().Be(config.StandardBookFormat);
        }

        [Test]
        public void should_get_bad_request_if_standard_format_is_empty()
        {
            IgnoreOnMonoVersions("5.12", "5.14");

            var config = NamingConfig.GetSingle();
            config.RenameBooks = true;
            config.StandardBookFormat = "";

            var errors = NamingConfig.InvalidPut(config);
            errors.Should().NotBeNull();
        }

        [Test]
        public void should_get_bad_request_if_standard_format_doesnt_contain_track_number_and_title()
        {
            IgnoreOnMonoVersions("5.12", "5.14");

            var config = NamingConfig.GetSingle();
            config.RenameBooks = true;
            config.StandardBookFormat = "{track:00}";

            var errors = NamingConfig.InvalidPut(config);
            errors.Should().NotBeNull();
        }

        [Test]
        public void should_not_require_format_when_rename_tracks_is_false()
        {
            IgnoreOnMonoVersions("5.12", "5.14");

            var config = NamingConfig.GetSingle();
            config.RenameBooks = false;
            config.StandardBookFormat = "";

            var errors = NamingConfig.InvalidPut(config);
            errors.Should().NotBeNull();
        }

        [Test]
        public void should_require_format_when_rename_tracks_is_true()
        {
            IgnoreOnMonoVersions("5.12", "5.14");

            var config = NamingConfig.GetSingle();
            config.RenameBooks = true;
            config.StandardBookFormat = "";

            var errors = NamingConfig.InvalidPut(config);
            errors.Should().NotBeNull();
        }

        [Test]
        public void should_get_bad_request_if_author_folder_format_does_not_contain_author_name()
        {
            IgnoreOnMonoVersions("5.12", "5.14");

            var config = NamingConfig.GetSingle();
            config.RenameBooks = true;
            config.AuthorFolderFormat = "This and That";

            var errors = NamingConfig.InvalidPut(config);
            errors.Should().NotBeNull();
        }
    }
}
