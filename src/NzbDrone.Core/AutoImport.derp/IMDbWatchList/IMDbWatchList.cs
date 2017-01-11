
using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tv;
using NzbDrone.Core.AutoImport;
using NzbDrone.Core.AutoImport.IMDbWatchList;
using System;

namespace NzbDrone.Core.AutoImpoter.IMDbWatchList
{
    public class IMDbWatchList : AutoImportBase<IMDbWatchListSettings>
    {
        public override bool Enabled
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        //private readonly INotifyMyAndroidProxy _proxy;

        //public NotifyMyAndroid(INotifyMyAndroidProxy proxy)
        //{
        //    _proxy = proxy;
        //}

        public override string Link => "http://rss.imdb.com/";

        public override string Name => "IMDb Public Watchlist";


        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            // failures.AddIfNotNull(_proxy.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
