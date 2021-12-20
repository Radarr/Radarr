using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Notifications;
using NzbDrone.Core.Notifications.Slack;
using NzbDrone.Core.ThingiProvider.Events;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(ProviderUpdatedEvent<INotificationFactory>))]
    [CheckOn(typeof(ProviderDeletedEvent<INotificationFactory>))]
    [CheckOn(typeof(ProviderStatusChangedEvent<INotificationFactory>))]
    public class SlackUrlCheck : HealthCheckBase
    {
        private readonly INotificationFactory _notificationFactory;

        public SlackUrlCheck(INotificationFactory notificationFactory, ILocalizationService localizationService)
            : base(localizationService)
        {
            _notificationFactory = notificationFactory;
        }

        public override HealthCheck Check()
        {
            var discordSlackNotifications = _notificationFactory.GetAvailableProviders().Where(n => n.ConfigContract.Equals("SlackSettings") && (n.Definition.Settings as SlackSettings).WebHookUrl.Contains("discord"));

            if (discordSlackNotifications.Empty())
            {
                return new HealthCheck(GetType());
            }

            return new HealthCheck(GetType(),
                HealthCheckResult.Warning,
                string.Format(_localizationService.GetLocalizedString("DiscordUrlInSlackNotification"),
                    string.Join(", ", discordSlackNotifications.Select(n => n.Name))),
                "#discord-as-slack-notification");
        }
    }
}
