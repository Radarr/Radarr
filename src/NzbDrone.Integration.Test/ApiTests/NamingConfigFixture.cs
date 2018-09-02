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
            config.RenameTracks = false;
            config.StandardTrackFormat = "{Artist Name} - {Album Title} - {track:00} - {Track Title}";

            var result = NamingConfig.Put(config);
            result.RenameTracks.Should().BeFalse();
            result.StandardTrackFormat.Should().Be(config.StandardTrackFormat);
        }

        [Test]
        public void should_get_bad_request_if_standard_format_is_empty()
        {
            var config = NamingConfig.GetSingle();
            config.RenameTracks = true;
            config.StandardTrackFormat = "";


            var errors = NamingConfig.InvalidPut(config);
            errors.Should().NotBeNull();
        }

        [Test]
        public void should_get_bad_request_if_standard_format_doesnt_contain_track_number_and_title()
        {
            var config = NamingConfig.GetSingle();
            config.RenameTracks = true;
            config.StandardTrackFormat = "{track:00}";

            var errors = NamingConfig.InvalidPut(config);
            errors.Should().NotBeNull();
        }

        [Test]
        public void should_not_require_format_when_rename_tracks_is_false()
        {
            var config = NamingConfig.GetSingle();
            config.RenameTracks = false;
            config.StandardTrackFormat = "";

            var errors = NamingConfig.InvalidPut(config);
            errors.Should().NotBeNull();
        }

        [Test]
        public void should_require_format_when_rename_tracks_is_true()
        {
            var config = NamingConfig.GetSingle();
            config.RenameTracks = true;
            config.StandardTrackFormat = "";

            var errors = NamingConfig.InvalidPut(config);
            errors.Should().NotBeNull();
        }

        [Test]
        public void should_get_bad_request_if_artist_folder_format_does_not_contain_artist_name()
        {
            var config = NamingConfig.GetSingle();
            config.RenameTracks = true;
            config.ArtistFolderFormat = "This and That";

            var errors = NamingConfig.InvalidPut(config);
            errors.Should().NotBeNull();
        }
    }
}
