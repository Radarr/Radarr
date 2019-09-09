using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http.Dispatchers;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Profiles.Qualities
{
    public interface IProfileService
    {
        QualityProfile Add(QualityProfile profile);
        void Update(QualityProfile profile);
        void Delete(int id);
        List<QualityProfile> All();
        QualityProfile Get(int id);
        bool Exists(int id);
        QualityProfile GetDefaultProfile(string name, Quality cutoff = null, params Quality[] allowed);

    }

    public class QualityProfileService : IProfileService, IHandle<ApplicationStartedEvent>
    {
        private readonly IProfileRepository _profileRepository;
        private readonly IArtistService _artistService;
        private readonly IImportListFactory _importListFactory;
        private readonly Logger _logger;

        public QualityProfileService(IProfileRepository profileRepository, IArtistService artistService, IImportListFactory importListFactory, Logger logger)
        {
            _profileRepository = profileRepository;
            _artistService = artistService;
            _importListFactory = importListFactory;
            _logger = logger;
        }

        public QualityProfile Add(QualityProfile profile)
        {
            return _profileRepository.Insert(profile);
        }

        public void Update(QualityProfile profile)
        {
            _profileRepository.Update(profile);
        }

        public void Delete(int id)
        {
            if (_artistService.GetAllArtists().Any(c => c.QualityProfileId == id) || _importListFactory.All().Any(c => c.ProfileId == id))
            {
                var profile = _profileRepository.Get(id);
                throw new QualityProfileInUseException(profile.Name);
            }

            _profileRepository.Delete(id);
        }

        public List<QualityProfile> All()
        {
            return _profileRepository.All().ToList();
        }

        public QualityProfile Get(int id)
        {
            return _profileRepository.Get(id);
        }

        public bool Exists(int id)
        {
            return _profileRepository.Exists(id);
        }

        public void Handle(ApplicationStartedEvent message)
        {
            if (All().Any()) return;

            _logger.Info("Setting up default quality profiles");

            AddDefaultProfile("Any", Quality.Unknown,
                Quality.Unknown,
                Quality.MP3_008,
                Quality.MP3_016,
                Quality.MP3_024,
                Quality.MP3_032,
                Quality.MP3_040,
                Quality.MP3_048,
                Quality.MP3_056,
                Quality.MP3_064,
                Quality.MP3_080,
                Quality.MP3_096,
                Quality.MP3_112,
                Quality.MP3_128,
                Quality.MP3_160,
                Quality.MP3_192,
                Quality.MP3_224,
                Quality.MP3_256,
                Quality.MP3_320,
                Quality.MP3_VBR,
                Quality.MP3_VBR_V2,
                Quality.AAC_192,
                Quality.AAC_256,
                Quality.AAC_320,
                Quality.AAC_VBR,
                Quality.VORBIS_Q5,
                Quality.VORBIS_Q6,
                Quality.VORBIS_Q7,
                Quality.VORBIS_Q8,
                Quality.VORBIS_Q9,
                Quality.VORBIS_Q10,
                Quality.WMA,
                Quality.ALAC,
                Quality.FLAC,
                Quality.FLAC_24);

            AddDefaultProfile("Lossless", Quality.FLAC,
                Quality.FLAC,
                Quality.ALAC,
                Quality.FLAC_24);

            AddDefaultProfile("Standard", Quality.MP3_192,
                Quality.MP3_192,
                Quality.MP3_256,
                Quality.MP3_320);
        }

        public QualityProfile GetDefaultProfile(string name, Quality cutoff = null, params Quality[] allowed)
        {
            var groupedQualites = Quality.DefaultQualityDefinitions.GroupBy(q => q.GroupWeight);
            var items = new List<QualityProfileQualityItem>();
            var groupId = 1000;
            var profileCutoff = cutoff == null ? Quality.Unknown.Id : cutoff.Id;

            foreach (var group in groupedQualites)
            {
                if (group.Count() == 1)
                {
                    var quality = group.First().Quality;
                    items.Add(new QualityProfileQualityItem { Quality = quality, Allowed = allowed.Contains(quality) });
                    continue;
                }

                var groupAllowed = group.Any(g => allowed.Contains(g.Quality));

                items.Add(new QualityProfileQualityItem
                {
                    Id = groupId,
                    Name = group.First().GroupName,
                    Items = group.Select(g => new QualityProfileQualityItem
                    {
                        Quality = g.Quality,
                        Allowed = groupAllowed
                    }).ToList(),
                    Allowed = groupAllowed
                });

                if (group.Any(s => s.Quality.Id == profileCutoff))
                {
                    profileCutoff = groupId;
                }

                groupId++;
            }

            var qualityProfile = new QualityProfile
            {
                Name = name,
                Cutoff = profileCutoff,
                Items = items
            };

            return qualityProfile;
        }

        private QualityProfile AddDefaultProfile(string name, Quality cutoff, params Quality[] allowed)
        {
            var profile = GetDefaultProfile(name, cutoff, allowed);

            return Add(profile);
        }
    }
}
