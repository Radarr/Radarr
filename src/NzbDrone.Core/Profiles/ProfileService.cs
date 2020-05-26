using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.CustomFormats.Events;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.NetImport;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Profiles
{
    public interface IProfileService
    {
        Profile Add(Profile profile);
        void Update(Profile profile);
        void Delete(int id);
        List<Profile> All();
        Profile Get(int id);
        bool Exists(int id);
        Profile GetDefaultProfile(string name, Quality cutoff = null, params Quality[] allowed);
        List<Language> GetAcceptableLanguages(int profileId);
    }

    public class ProfileService : IProfileService,
        IHandle<ApplicationStartedEvent>,
        IHandle<CustomFormatAddedEvent>,
        IHandle<CustomFormatDeletedEvent>
    {
        private readonly IProfileRepository _profileRepository;
        private readonly ICustomFormatService _formatService;
        private readonly IMovieService _movieService;
        private readonly INetImportFactory _netImportFactory;
        private readonly Logger _logger;

        public ProfileService(IProfileRepository profileRepository,
                              ICustomFormatService formatService,
                              IMovieService movieService,
                              INetImportFactory netImportFactory,
                              Logger logger)
        {
            _profileRepository = profileRepository;
            _formatService = formatService;
            _movieService = movieService;
            _netImportFactory = netImportFactory;
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

        public void Handle(CustomFormatAddedEvent message)
        {
            var all = All();
            foreach (var profile in all)
            {
                profile.FormatItems.Insert(0, new ProfileFormatItem
                {
                    Score = 0,
                    Format = message.CustomFormat
                });

                Update(profile);
            }
        }

        public void Handle(CustomFormatDeletedEvent message)
        {
            var all = All();
            foreach (var profile in all)
            {
                profile.FormatItems = profile.FormatItems.Where(c => c.Format.Id != message.CustomFormat.Id).ToList();
                if (!profile.FormatItems.Any())
                {
                    profile.MinFormatScore = 0;
                    profile.CutoffFormatScore = 0;
                }

                Update(profile);
            }
        }

        public void Handle(ApplicationStartedEvent message)
        {
            if (All().Any())
            {
                return;
            }

            _logger.Info("Setting up default quality profiles");

            AddDefaultProfile("Any",
                Quality.Bluray480p,
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
                Quality.WEBRip480p,
                Quality.WEBDL720p,
                Quality.WEBRip720p,
                Quality.WEBDL1080p,
                Quality.WEBRip1080p,
                Quality.WEBDL2160p,
                Quality.WEBRip2160p,
                Quality.Bluray480p,
                Quality.Bluray576p,
                Quality.Bluray720p,
                Quality.Bluray1080p,
                Quality.Bluray2160p,
                Quality.Remux1080p,
                Quality.Remux2160p,
                Quality.BRDISK);

            AddDefaultProfile("SD",
                Quality.Bluray480p,
                Quality.WORKPRINT,
                Quality.CAM,
                Quality.TELESYNC,
                Quality.TELECINE,
                Quality.DVDSCR,
                Quality.REGIONAL,
                Quality.SDTV,
                Quality.DVD,
                Quality.WEBDL480p,
                Quality.WEBRip480p,
                Quality.Bluray480p,
                Quality.Bluray576p);

            AddDefaultProfile("HD-720p",
                Quality.Bluray720p,
                Quality.HDTV720p,
                Quality.WEBDL720p,
                Quality.WEBRip720p,
                Quality.Bluray720p);

            AddDefaultProfile("HD-1080p",
                Quality.Bluray1080p,
                Quality.HDTV1080p,
                Quality.WEBDL1080p,
                Quality.WEBRip1080p,
                Quality.Bluray1080p,
                Quality.Remux1080p);

            AddDefaultProfile("Ultra-HD",
                Quality.Remux2160p,
                Quality.HDTV2160p,
                Quality.WEBDL2160p,
                Quality.WEBRip2160p,
                Quality.Bluray2160p,
                Quality.Remux2160p);

            AddDefaultProfile("HD - 720p/1080p",
                Quality.Bluray720p,
                Quality.HDTV720p,
                Quality.HDTV1080p,
                Quality.WEBDL720p,
                Quality.WEBRip720p,
                Quality.WEBDL1080p,
                Quality.WEBRip1080p,
                Quality.Bluray720p,
                Quality.Bluray1080p,
                Quality.Remux1080p,
                Quality.Remux2160p);
        }

        public Profile GetDefaultProfile(string name, Quality cutoff = null, params Quality[] allowed)
        {
            var groupedQualites = Quality.DefaultQualityDefinitions.GroupBy(q => q.Weight);
            var items = new List<ProfileQualityItem>();
            var groupId = 1000;
            var profileCutoff = cutoff == null ? Quality.Unknown.Id : cutoff.Id;

            foreach (var group in groupedQualites)
            {
                if (group.Count() == 1)
                {
                    var quality = group.First().Quality;

                    items.Add(new ProfileQualityItem { Quality = group.First().Quality, Allowed = allowed.Contains(quality) });
                    continue;
                }

                var groupAllowed = group.Any(g => allowed.Contains(g.Quality));

                items.Add(new ProfileQualityItem
                {
                    Id = groupId,
                    Name = group.First().GroupName,
                    Items = group.Select(g => new ProfileQualityItem
                    {
                        Quality = g.Quality,
                        Allowed = groupAllowed
                    }).ToList(),
                    Allowed = groupAllowed
                });

                if (group.Any(g => g.Quality.Id == profileCutoff))
                {
                    profileCutoff = groupId;
                }

                groupId++;
            }

            var formatItems = _formatService.All().Select(format => new ProfileFormatItem
            {
                Id = format.Id,
                Score = 0,
                Format = format
            }).ToList();

            var qualityProfile = new Profile
            {
                Name = name,
                Cutoff = profileCutoff,
                Items = items,
                Language = Language.English,
                MinFormatScore = 0,
                CutoffFormatScore = 0,
                FormatItems = formatItems
            };

            return qualityProfile;
        }

        public List<Language> GetAcceptableLanguages(int profileId)
        {
            var profile = Get(profileId);

            var wantedTitleLanguages = profile.FormatItems.Where(i => i.Score > 0).Select(item => item.Format)
                .SelectMany(format => format.Specifications)
                .Where(specification => specification is LanguageSpecification && !specification.Negate)
                .Cast<LanguageSpecification>()
                .Where(specification => specification.Value > 0)
                .Select(specification => (Language)specification.Value)
                .Distinct()
                .ToList();

            wantedTitleLanguages.Add(profile.Language);

            return wantedTitleLanguages;
        }

        private Profile AddDefaultProfile(string name, Quality cutoff, params Quality[] allowed)
        {
            var profile = GetDefaultProfile(name, cutoff, allowed);

            return Add(profile);
        }
    }
}
