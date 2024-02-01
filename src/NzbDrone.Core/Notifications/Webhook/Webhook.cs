using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Localization;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Tags;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class Webhook : WebhookBase<WebhookSettings>
    {
        private readonly IWebhookProxy _proxy;

        public Webhook(IWebhookProxy proxy, IConfigFileProvider configFileProvider, IConfigService configService, ILocalizationService localizationService, ITagRepository tagRepository)
            : base(configFileProvider, configService, localizationService, tagRepository)
        {
            _proxy = proxy;
        }

        public override string Link => "https://wiki.servarr.com/radarr/settings#connect";

        public override void OnGrab(GrabMessage message)
        {
            _proxy.SendWebhook(BuildOnGrabPayload(message), Settings);
        }

        public override void OnDownload(DownloadMessage message)
        {
            _proxy.SendWebhook(BuildOnDownloadPayload(message), Settings);
        }

        public override void OnMovieRename(Movie movie, List<RenamedMovieFile> renamedFiles)
        {
            _proxy.SendWebhook(BuildOnRenamePayload(movie, renamedFiles), Settings);
        }

        public override void OnMovieAdded(Movie movie)
        {
            _proxy.SendWebhook(BuildOnMovieAdded(movie), Settings);
        }

        public override void OnMovieFileDelete(MovieFileDeleteMessage deleteMessage)
        {
            _proxy.SendWebhook(BuildOnMovieFileDelete(deleteMessage), Settings);
        }

        public override void OnMovieDelete(MovieDeleteMessage deleteMessage)
        {
            _proxy.SendWebhook(BuildOnMovieDelete(deleteMessage), Settings);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
            _proxy.SendWebhook(BuildHealthPayload(healthCheck), Settings);
        }

        public override void OnHealthRestored(HealthCheck.HealthCheck previousCheck)
        {
            _proxy.SendWebhook(BuildHealthRestoredPayload(previousCheck), Settings);
        }

        public override void OnApplicationUpdate(ApplicationUpdateMessage updateMessage)
        {
            _proxy.SendWebhook(BuildApplicationUpdatePayload(updateMessage), Settings);
        }

        public override void OnManualInteractionRequired(ManualInteractionRequiredMessage message)
        {
            _proxy.SendWebhook(BuildManualInteractionRequiredPayload(message), Settings);
        }

        public override string Name => "Webhook";

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(SendWebhookTest());

            return new ValidationResult(failures);
        }

        private ValidationFailure SendWebhookTest()
        {
            try
            {
                _proxy.SendWebhook(BuildTestPayload(), Settings);
            }
            catch (WebhookException ex)
            {
                return new NzbDroneValidationFailure("Url", _localizationService.GetLocalizedString("NotificationsValidationUnableToSendTestMessage", new Dictionary<string, object> { { "exceptionMessage", ex.Message } }));
            }

            return null;
        }
    }
}
