using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Movies;
using NzbDrone.Core.NetImport.ListMovies;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.NetImport.Radarr
{
    public class RadarrImport : NetImportBase<RadarrSettings>
    {
        private readonly IRadarrV3Proxy _radarrV3Proxy;
        public override string Name => "Radarr";
        public override bool Enabled => true;
        public override bool EnableAuto => false;

        public override NetImportType ListType => NetImportType.Program;

        public RadarrImport(IRadarrV3Proxy radarrV3Proxy,
                            INetImportStatusService netImportStatusService,
                            IConfigService configService,
                            IParsingService parsingService,
                            Logger logger)
            : base(netImportStatusService, configService, parsingService, logger)
        {
            _radarrV3Proxy = radarrV3Proxy;
        }

        public override NetImportFetchResult Fetch()
        {
            var movies = new List<ListMovie>();
            var anyFailure = false;

            try
            {
                var remoteMovies = _radarrV3Proxy.GetMovies(Settings);

                foreach (var remoteMovie in remoteMovies)
                {
                    if (!Settings.ProfileIds.Any() || Settings.ProfileIds.Contains(remoteMovie.QualityProfileId))
                    {
                        movies.Add(new ListMovie
                        {
                            TmdbId = remoteMovie.TmdbId,
                            Title = remoteMovie.Title,
                            SortTitle = remoteMovie.SortTitle,
                            Overview = remoteMovie.Overview,
                            Images = remoteMovie.Images.Select(x => MapImage(x, Settings.BaseUrl)).ToList(),
                            PhysicalRelease = remoteMovie.PhysicalRelease,
                            InCinemas = remoteMovie.InCinemas,
                            Year = remoteMovie.Year
                        });
                    }
                }

                _netImportStatusService.RecordSuccess(Definition.Id);
            }
            catch
            {
                anyFailure = true;
                _netImportStatusService.RecordFailure(Definition.Id);
            }

            return new NetImportFetchResult { Movies = CleanupListItems(movies), AnyFailure = anyFailure };
        }

        public override object RequestAction(string action, IDictionary<string, string> query)
        {
            if (action == "getDevices")
            {
                // Return early if there is not an API key
                if (Settings.ApiKey.IsNullOrWhiteSpace())
                {
                    return new
                    {
                        devices = new List<object>()
                    };
                }

                Settings.Validate().Filter("ApiKey").ThrowOnError();

                var devices = _radarrV3Proxy.GetProfiles(Settings);

                return new
                {
                    options = devices.OrderBy(d => d.Name, StringComparer.InvariantCultureIgnoreCase)
                                            .Select(d => new
                                            {
                                                id = d.Id,
                                                name = d.Name
                                            })
                };
            }

            return new { };
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(_radarrV3Proxy.Test(Settings));
        }

        private static MediaCover.MediaCover MapImage(MediaCover.MediaCover arg, string baseUrl)
        {
            var newImage = new MediaCover.MediaCover
            {
                Url = string.Format("{0}{1}", baseUrl, arg.Url),
                CoverType = arg.CoverType
            };

            return newImage;
        }
    }
}
