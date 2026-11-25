using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Notifications;
using NzbDrone.Core.Notifications.Gotify;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.NotificationTests
{
    [TestFixture]
    public class GotifyServiceFixture : CoreTest<Gotify>
    {
        [SetUp]
        public void Setup()
        {
            Subject.Definition = new NotificationDefinition
            {
                Settings = new GotifySettings
                {
                    Server = "https://example.invalid",
                    AppToken = "token",
                    Priority = 5,
                    IncludeMoviePoster = false,
                    IncludeInstanceNameInTitle = false,
                    MetadataLinks = Enumerable.Empty<int>(),
                    PreferredMetadataLink = (int)MetadataLinkType.Tmdb
                }
            };
        }

        [TestCase(false, "MyRadarr", false)]
        [TestCase(true, "MyRadarr", true)]
        [TestCase(true, "", false)]
        [TestCase(true, "   ", false)]
        public void OnDownload_should_append_instance_name_to_title_only_when_enabled_and_non_empty(bool includeInstanceNameInTitle, string instanceName, bool shouldAppendInstanceName)
        {
            ((GotifySettings)Subject.Definition.Settings).IncludeInstanceNameInTitle = includeInstanceNameInTitle;

            Mocker.GetMock<IConfigFileProvider>()
                  .SetupGet(c => c.InstanceName)
                  .Returns(instanceName);

            var message = new DownloadMessage
            {
                Movie = new Movie { Title = "Movie" },
                Message = "downloaded"
            };

            Subject.OnDownload(message);

            var suffix = $" - {instanceName}";

            Predicate<GotifyMessage> titleHasCorrectSuffix = m =>
                shouldAppendInstanceName
                    ? m.Title.EndsWith(suffix)
                    : !m.Title.EndsWith(" - ") && !m.Title.EndsWith(suffix);

            Mocker.GetMock<IGotifyProxy>()
                .Verify(p => p.SendNotification(
                        It.Is<GotifyMessage>(m => titleHasCorrectSuffix(m)),
                        It.IsAny<GotifySettings>()),
                    Times.Once());
        }
    }
}
