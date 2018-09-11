using System.Collections.Generic;
using System.Linq;
using NLog;
 using NzbDrone.Core.CustomFormats;
 using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Movies;
using NzbDrone.Core.NetImport;

namespace NzbDrone.Core.Profiles
{
    public interface IProfileService
    {
        Profile Add(Profile profile);
        void Update(Profile profile);
        void AddCustomFormat(CustomFormat format);
        void DeleteCustomFormat(int formatId);
        void Delete(int id);
        List<Profile> All();
        Profile Get(int id);
        bool Exists(int id);
    }

    public class ProfileService : IProfileService, IHandle<ApplicationStartedEvent>
    {
        private readonly IProfileRepository _profileRepository;
        private readonly IMovieService _movieService;
        private readonly INetImportFactory _netImportFactory;
        private readonly ICustomFormatService _formatService;
        private readonly Logger _logger;

        public ProfileService(IProfileRepository profileRepository, IMovieService movieService,
            INetImportFactory netImportFactory, ICustomFormatService formatService, Logger logger)
        {
            _profileRepository = profileRepository;
            _movieService = movieService;
            _netImportFactory = netImportFactory;
            _formatService = formatService;
            _logger = logger;
        }

        public Profile Add(Profile profile)
        {
            return _profileRepository.Insert(profile);
        }

        public void Update(Profile profile)
        {
            _profileRepository.Update(profile);
        }

        public void AddCustomFormat(CustomFormat customFormat)
        {
            var all = All();
            foreach (var profile in all)
            {
                profile.FormatItems.Add(new ProfileFormatItem
                {
                    Allowed = true,
                    Format = customFormat
                });

                Update(profile);
            }
        }

        public void DeleteCustomFormat(int formatId)
        {
            var all = All();
            foreach (var profile in all)
            {
                profile.FormatItems = profile.FormatItems.Where(c => c.Format.Id != formatId).ToList();
                if (profile.FormatCutoff.Id == formatId)
                {
                    profile.FormatCutoff = CustomFormat.None;
                }

                Update(profile);
            }
        }

        public void Delete(int id)
        {
            if (_movieService.GetAllMovies().Any(c => c.ProfileId == id) || _netImportFactory.All().Any(c => c.ProfileId == id))
            {
                throw new ProfileInUseException(id);
            }

            _profileRepository.Delete(id);
        }

        public List<Profile> All()
        {
            return _profileRepository.All().ToList();
        }

        public Profile Get(int id)
        {
            return _profileRepository.Get(id);
        }

        public bool Exists(int id)
        {
            return _profileRepository.Exists(id);
        }

        private Profile AddDefaultProfile(string name, Quality cutoff, params Quality[] allowed)
        {
            var items = Quality.DefaultQualityDefinitions
                            .OrderBy(v => v.Weight)
                            .Select(v => new ProfileQualityItem { Quality = v.Quality, Allowed = allowed.Contains(v.Quality) })
                            .ToList();

            var profile = new Profile { Name = name, Cutoff = cutoff, Items = items, Language = Language.English, FormatCutoff = CustomFormat.None, FormatItems = new List<ProfileFormatItem>
            {
                new ProfileFormatItem
                {
                    Allowed = true,
                    Format = CustomFormat.None
                }
            }};

            return Add(profile);
        }

        public void Handle(ApplicationStartedEvent message)
        {
            // Hack to force custom formats to be loaded into memory, if you have a better solution please let me know.
            _formatService.All();
            if (All().Any()) return;

            _logger.Info("Setting up default quality profiles");

            AddDefaultProfile("Any", Quality.Bluray480p,
                Quality.WORKPRINT,
                Quality.CAM,
                Quality.TELESYNC,
                Quality.TELECINE,
                Quality.DVDSCR,
                Quality.REGIONAL,
                Quality.SDTV,
                Quality.DVD,
                Quality.DVDR,
                Quality.HDTV720p,
                Quality.HDTV1080p,
                Quality.HDTV2160p,
                Quality.WEBDL480p,
                Quality.WEBDL720p,
                Quality.WEBDL1080p,
                Quality.WEBDL2160p,
                Quality.Bluray480p,
                Quality.Bluray576p,
                Quality.Bluray720p,
                Quality.Bluray1080p,
                Quality.Bluray2160p,
                Quality.Remux1080p,
                Quality.Remux2160p,
                Quality.BRDISK);

            AddDefaultProfile("SD", Quality.Bluray480p,
                Quality.WORKPRINT,
                Quality.CAM,
                Quality.TELESYNC,
                Quality.TELECINE,
                Quality.DVDSCR,
                Quality.REGIONAL,
                Quality.SDTV,
                Quality.DVD,
                Quality.WEBDL480p,
                Quality.Bluray480p,
                Quality.Bluray576p);

            AddDefaultProfile("HD-720p", Quality.Bluray720p,
                Quality.HDTV720p,
                Quality.WEBDL720p,
                Quality.Bluray720p);

            AddDefaultProfile("HD-1080p", Quality.Bluray1080p,
                Quality.HDTV1080p,
                Quality.WEBDL1080p,
                Quality.Bluray1080p,
                Quality.Remux1080p);

            AddDefaultProfile("Ultra-HD", Quality.Remux2160p,
                Quality.HDTV2160p,
                Quality.WEBDL2160p,
                Quality.Bluray2160p,
                Quality.Remux2160p);

            AddDefaultProfile("HD - 720p/1080p", Quality.Bluray720p,
                Quality.HDTV720p,
                Quality.HDTV1080p,
                Quality.WEBDL720p,
                Quality.WEBDL1080p,
                Quality.Bluray720p,
                Quality.Bluray1080p,
                Quality.Remux1080p,
                Quality.Remux2160p
                );
        }
    }
}
