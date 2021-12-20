using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class migrate_discord_from_slackFixture : MigrationTest<migrate_discord_from_slack>
    {
        private readonly JsonSerializerOptions _serializerSettings;

        public migrate_discord_from_slackFixture()
        {
            _serializerSettings = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                PropertyNameCaseInsensitive = true,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            _serializerSettings.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, true));
        }

        [Test]
        public void should_replace_old_url()
        {
            var webhookUrl = "https://discord.com/api/webhooks/922499153416847361/f9CAcD5i_E_-0AoPfMVa8igVK8h271HpJDbd6euUrPh9KonWlMCziLOSMmD-2SQ4CHmX/slack";
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Notifications").Row(new
                {
                    Name = "SlackDiscord",
                    Implementation = "Slack",
                    Settings = new SlackNotificationSettings201
                    {
                        Icon = "TestURL",
                        Username = "TestUsername",
                        WebHookUrl = webhookUrl
                    }.ToJson(),
                    ConfigContract = "SlackSettings",
                    OnGrab = true,
                    OnDownload = true,
                    OnUpgrade = true,
                    OnRename = true,
                    OnHealthIssue = true,
                    OnMovieDelete = true,
                    OnMovieFileDelete = true,
                    OnMovieFileDeleteForUpgrade = true,
                    IncludeHealthWarnings = true
                });
            });

            var items = db.Query<NotificationEntity201>("SELECT Id,ConfigContract,Implementation,Name,Settings FROM Notifications");

            items.Should().HaveCount(1);
            items.First().ConfigContract.Should().Be("DiscordSettings");
            var settings = JsonSerializer.Deserialize<DiscordNotificationSettings201>(items.First().Settings, _serializerSettings);
            settings.Avatar.Should().Be("TestURL");
            settings.Username.Should().Be("TestUsername");
            settings.WebHookUrl.Should().Be(webhookUrl.Replace("/slack", ""));
        }
    }
}
