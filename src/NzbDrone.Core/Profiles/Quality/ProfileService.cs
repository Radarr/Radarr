using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Profiles.Qualities
{
    public interface IProfileService
    {
        Profile Add(Profile profile);
        void Update(Profile profile);
        void Delete(int id);
        List<Profile> All();
        Profile Get(int id);
        bool Exists(int id);
    }

    public class ProfileService : IProfileService, IHandle<ApplicationStartedEvent>
    {
        private readonly IProfileRepository _profileRepository;
        private readonly IArtistService _artistService;
        private readonly Logger _logger;

        public ProfileService(IProfileRepository profileRepository, IArtistService artistService, Logger logger)
        {
            _profileRepository = profileRepository;
            _artistService = artistService;
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
            if (_artistService.GetAllArtists().Any(c => c.ProfileId == id))
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

            var profile = new Profile { Name = name,
                Cutoff = (int)cutoff,
                Items = items};

            return Add(profile);
        }

        public void Handle(ApplicationStartedEvent message)
        {
            if (All().Any()) return;

            _logger.Info("Setting up default quality profiles");

            AddDefaultProfile("Any", 
                Quality.Unknown,
                Quality.Unknown,
                Quality.MP3_192,
                Quality.MP3_256,
                Quality.MP3_320,
                Quality.MP3_512,
                Quality.MP3_VBR,
                Quality.FLAC);

            AddDefaultProfile("Lossless",
                Quality.FLAC,
                Quality.FLAC);

            AddDefaultProfile("Standard",
                Quality.MP3_192,
                Quality.MP3_192,
                Quality.MP3_256,
                Quality.MP3_320);
        }
    }
}
