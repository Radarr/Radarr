using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Books;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.RootFolders;

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
        private readonly IAuthorService _authorService;
        private readonly IImportListFactory _importListFactory;
        private readonly IRootFolderService _rootFolderService;
        private readonly Logger _logger;

        public QualityProfileService(IProfileRepository profileRepository,
                                     IAuthorService authorService,
                                     IImportListFactory importListFactory,
                                     IRootFolderService rootFolderService,
                                     Logger logger)
        {
            _profileRepository = profileRepository;
            _authorService = authorService;
            _importListFactory = importListFactory;
            _rootFolderService = rootFolderService;
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
            if (_authorService.GetAllAuthors().Any(c => c.QualityProfileId == id) ||
                _importListFactory.All().Any(c => c.ProfileId == id) ||
                _rootFolderService.All().Any(c => c.DefaultQualityProfileId == id))
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
            if (All().Any())
            {
                return;
            }

            _logger.Info("Setting up default quality profiles");

            AddDefaultProfile("Any",
                              Quality.Unknown,
                              Quality.Unknown,
                              Quality.PDF,
                              Quality.MOBI,
                              Quality.EPUB,
                              Quality.AZW3,
                              Quality.MP3_320,
                              Quality.FLAC);

            AddDefaultProfile("Lossless Audio",
                              Quality.FLAC,
                              Quality.FLAC);

            AddDefaultProfile("Standard Audio",
                              Quality.MP3_320,
                              Quality.MP3_320);

            AddDefaultProfile("Text",
                              Quality.MOBI,
                              Quality.MOBI,
                              Quality.EPUB,
                              Quality.AZW3);
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
