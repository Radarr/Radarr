using System;
using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Notifications
{
    public abstract class NotificationBase<TSettings> : INotification
        where TSettings : NotificationSettingsBase<TSettings>, new()
    {
        protected const string MOVIE_GRABBED_TITLE = "Movie Grabbed";
        protected const string MOVIE_DOWNLOADED_TITLE = "Movie Downloaded";
        protected const string MOVIE_UPGRADED_TITLE = "Movie Upgraded";
        protected const string MOVIE_ADDED_TITLE = "Movie Added";
        protected const string MOVIE_DELETED_TITLE = "Movie Deleted";
        protected const string MOVIE_FILE_DELETED_TITLE = "Movie File Deleted";
        protected const string HEALTH_ISSUE_TITLE = "Health Check Failure";
        protected const string HEALTH_RESTORED_TITLE = "Health Check Restored";
        protected const string APPLICATION_UPDATE_TITLE = "Application Updated";
        protected const string MANUAL_INTERACTION_REQUIRED_TITLE = "Manual Interaction";

        protected const string MOVIE_GRABBED_TITLE_BRANDED = "Radarr - " + MOVIE_GRABBED_TITLE;
        protected const string MOVIE_ADDED_TITLE_BRANDED = "Radarr - " + MOVIE_ADDED_TITLE;
        protected const string MOVIE_DOWNLOADED_TITLE_BRANDED = "Radarr - " + MOVIE_DOWNLOADED_TITLE;
        protected const string MOVIE_UPGRADED_TITLE_BRANDED = "Radarr - " + MOVIE_UPGRADED_TITLE;
        protected const string MOVIE_DELETED_TITLE_BRANDED = "Radarr - " + MOVIE_DELETED_TITLE;
        protected const string MOVIE_FILE_DELETED_TITLE_BRANDED = "Radarr - " + MOVIE_FILE_DELETED_TITLE;
        protected const string HEALTH_ISSUE_TITLE_BRANDED = "Radarr - " + HEALTH_ISSUE_TITLE;
        protected const string HEALTH_RESTORED_TITLE_BRANDED = "Radarr - " + HEALTH_RESTORED_TITLE;
        protected const string APPLICATION_UPDATE_TITLE_BRANDED = "Radarr - " + APPLICATION_UPDATE_TITLE;
        protected const string MANUAL_INTERACTION_REQUIRED_TITLE_BRANDED = "Radarr - " + MANUAL_INTERACTION_REQUIRED_TITLE;

        public abstract string Name { get; }

        public Type ConfigContract => typeof(TSettings);

        public virtual ProviderMessage Message => null;

        public IEnumerable<ProviderDefinition> DefaultDefinitions => new List<ProviderDefinition>();

        public ProviderDefinition Definition { get; set; }
        public abstract ValidationResult Test();

        public abstract string Link { get; }

        public virtual void OnGrab(GrabMessage grabMessage)
        {
        }

        public virtual void OnDownload(DownloadMessage message)
        {
        }

        public virtual void OnMovieRename(Movie movie, List<RenamedMovieFile> renamedFiles)
        {
        }

        public virtual void OnMovieAdded(Movie movie)
        {
        }

        public virtual void OnMovieFileDelete(MovieFileDeleteMessage deleteMessage)
        {
        }

        public virtual void OnMovieDelete(MovieDeleteMessage deleteMessage)
        {
        }

        public virtual void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
        }

        public virtual void OnHealthRestored(HealthCheck.HealthCheck previousCheck)
        {
        }

        public virtual void OnApplicationUpdate(ApplicationUpdateMessage updateMessage)
        {
        }

        public virtual void OnManualInteractionRequired(ManualInteractionRequiredMessage message)
        {
        }

        public virtual void ProcessQueue()
        {
        }

        public bool SupportsOnGrab => HasConcreteImplementation("OnGrab");
        public bool SupportsOnRename => HasConcreteImplementation("OnMovieRename");
        public bool SupportsOnDownload => HasConcreteImplementation("OnDownload");
        public bool SupportsOnUpgrade => SupportsOnDownload;
        public bool SupportsOnMovieAdded => HasConcreteImplementation("OnMovieAdded");
        public bool SupportsOnMovieDelete => HasConcreteImplementation("OnMovieDelete");
        public bool SupportsOnMovieFileDelete => HasConcreteImplementation("OnMovieFileDelete");
        public bool SupportsOnMovieFileDeleteForUpgrade => SupportsOnMovieFileDelete;
        public bool SupportsOnHealthIssue => HasConcreteImplementation("OnHealthIssue");
        public bool SupportsOnHealthRestored => HasConcreteImplementation("OnHealthRestored");
        public bool SupportsOnApplicationUpdate => HasConcreteImplementation("OnApplicationUpdate");
        public bool SupportsOnManualInteractionRequired => HasConcreteImplementation("OnManualInteractionRequired");

        protected TSettings Settings => (TSettings)Definition.Settings;

        public override string ToString()
        {
            return GetType().Name;
        }

        public virtual object RequestAction(string action, IDictionary<string, string> query)
        {
            return null;
        }

        private bool HasConcreteImplementation(string methodName)
        {
            var method = GetType().GetMethod(methodName);

            if (method == null)
            {
                throw new MissingMethodException(GetType().Name, Name);
            }

            return !method.DeclaringType.IsAbstract;
        }
    }
}
