﻿using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Growl
{
    public class Growl : NotificationBase<GrowlSettings>
    {
        private readonly IGrowlService _growlService;

        public Growl(IGrowlService growlService)
        {
            _growlService = growlService;
        }

        public override string Link => "http://growl.info/";

        public override void OnGrab(GrabMessage grabMessage)
        {
            const string title = "Movie Grabbed";

            _growlService.SendNotification(title, grabMessage.Message, "GRAB", Settings.Host, Settings.Port, Settings.Password);
        }

        public override void OnDownload(DownloadMessage message)
        {
            const string title = "Movie Downloaded";

            _growlService.SendNotification(title, message.Message, "DOWNLOAD", Settings.Host, Settings.Port, Settings.Password);
        }

        public override void OnMovieRename(Movie movie)
        {
        }
		
        public override string Name => "Growl";

        public override bool SupportsOnRename => false;

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_growlService.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
