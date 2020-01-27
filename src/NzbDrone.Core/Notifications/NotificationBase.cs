using System;
using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Core.Movies;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Notifications
{
    public abstract class NotificationBase<TSettings> : INotification
        where TSettings : IProviderConfig, new()
    {
        protected const string MOVIE_GRABBED_TITLE = "Movie Grabbed";
        protected const string MOVIE_DOWNLOADED_TITLE = "Movie Downloaded";
        protected const string HEALTH_ISSUE_TITLE = "Health Check Failure";

        protected const string MOVIE_GRABBED_TITLE_BRANDED = "Radarr - " + MOVIE_GRABBED_TITLE;
        protected const string MOVIE_DOWNLOADED_TITLE_BRANDED = "Radarr - " + MOVIE_DOWNLOADED_TITLE;
        protected const string HEALTH_ISSUE_TITLE_BRANDED = "Radarr - " + HEALTH_ISSUE_TITLE;

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

        public virtual void OnMovieRename(Movie movie)
        {
        }

        public virtual void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
        }

        public bool SupportsOnGrab => HasConcreteImplementation("OnGrab");
        public bool SupportsOnRename => HasConcreteImplementation("OnMovieRename");
        public bool SupportsOnDownload => HasConcreteImplementation("OnDownload");
        public bool SupportsOnUpgrade => SupportsOnDownload;
        public bool SupportsOnHealthIssue => HasConcreteImplementation("OnHealthIssue");

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
