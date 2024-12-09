using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Notifications.Telegram
{
    public class Telegram : NotificationBase<TelegramSettings>
    {
        private readonly ITelegramProxy _proxy;
        private readonly IConfigFileProvider _configFileProvider;

        public Telegram(ITelegramProxy proxy, IConfigFileProvider configFileProvider)
        {
            _proxy = proxy;
            _configFileProvider = configFileProvider;
        }

        public override string Name => "Telegram";
        public override string Link => "https://telegram.org/";

        private string InstanceName => _configFileProvider.InstanceName;

        public override void OnGrab(GrabMessage grabMessage)
        {
            var title = Settings.IncludeAppNameInTitle ? MOVIE_GRABBED_TITLE_BRANDED : MOVIE_GRABBED_TITLE;
            title = Settings.IncludeInstanceNameInTitle ? $"{title} - {InstanceName}" : title;

            _proxy.SendNotification(title, grabMessage.Message, Settings);
        }

        public override void OnDownload(DownloadMessage message)
        {
            string title;
            if (message.OldMovieFiles.Any())
            {
                title = Settings.IncludeAppNameInTitle ? MOVIE_UPGRADED_TITLE_BRANDED : MOVIE_UPGRADED_TITLE;
            }
            else
            {
                title = Settings.IncludeAppNameInTitle ? MOVIE_DOWNLOADED_TITLE_BRANDED : MOVIE_DOWNLOADED_TITLE;
            }

            title = Settings.IncludeInstanceNameInTitle ? $"{title} - {InstanceName}" : title;

            _proxy.SendNotification(title, message.Message, Settings);
        }

        public override void OnMovieAdded(Movie movie)
        {
            var title = Settings.IncludeAppNameInTitle ? MOVIE_ADDED_TITLE_BRANDED : MOVIE_ADDED_TITLE;
            title = Settings.IncludeInstanceNameInTitle ? $"{title} - {InstanceName}" : title;

            _proxy.SendNotification(title, $"{movie.Title} added to library", Settings);
        }

        public override void OnMovieFileDelete(MovieFileDeleteMessage deleteMessage)
        {
            var title = Settings.IncludeAppNameInTitle ? MOVIE_FILE_DELETED_TITLE_BRANDED : MOVIE_FILE_DELETED_TITLE;
            title = Settings.IncludeInstanceNameInTitle ? $"{title} - {InstanceName}" : title;

            _proxy.SendNotification(title, deleteMessage.Message, Settings);
        }

        public override void OnMovieDelete(MovieDeleteMessage deleteMessage)
        {
            var title = Settings.IncludeAppNameInTitle ? MOVIE_DELETED_TITLE_BRANDED : MOVIE_DELETED_TITLE;
            title = Settings.IncludeInstanceNameInTitle ? $"{title} - {InstanceName}" : title;

            _proxy.SendNotification(title, deleteMessage.Message, Settings);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
            var title = Settings.IncludeAppNameInTitle ? HEALTH_ISSUE_TITLE_BRANDED : HEALTH_ISSUE_TITLE;
            title = Settings.IncludeInstanceNameInTitle ? $"{title} - {InstanceName}" : title;

            _proxy.SendNotification(title, healthCheck.Message, Settings);
        }

        public override void OnHealthRestored(HealthCheck.HealthCheck previousCheck)
        {
            var title = Settings.IncludeAppNameInTitle ? HEALTH_RESTORED_TITLE_BRANDED : HEALTH_RESTORED_TITLE;
            title = Settings.IncludeInstanceNameInTitle ? $"{title} - {InstanceName}" : title;

            _proxy.SendNotification(title, $"The following issue is now resolved: {previousCheck.Message}", Settings);
        }

        public override void OnApplicationUpdate(ApplicationUpdateMessage updateMessage)
        {
            var title = Settings.IncludeAppNameInTitle ? APPLICATION_UPDATE_TITLE_BRANDED : APPLICATION_UPDATE_TITLE;
            title = Settings.IncludeInstanceNameInTitle ? $"{title} - {InstanceName}" : title;

            _proxy.SendNotification(title, updateMessage.Message, Settings);
        }

        public override void OnManualInteractionRequired(ManualInteractionRequiredMessage message)
        {
            var title = Settings.IncludeAppNameInTitle ? MANUAL_INTERACTION_REQUIRED_TITLE_BRANDED : MANUAL_INTERACTION_REQUIRED_TITLE;
            title = Settings.IncludeInstanceNameInTitle ? $"{title} - {InstanceName}" : title;

            _proxy.SendNotification(title, message.Message, Settings);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_proxy.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
