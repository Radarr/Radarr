using NLog;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.ImportLists;
using NzbDrone.Common.Extensions;
using System;

namespace NzbDrone.Core.Profiles.Metadata
{
    public interface IMetadataProfileService
    {
        MetadataProfile Add(MetadataProfile profile);
        void Update(MetadataProfile profile);
        void Delete(int id);
        List<MetadataProfile> All();
        MetadataProfile Get(int id);
        bool Exists(int id);
    }

    public class MetadataProfileService : IMetadataProfileService, IHandle<ApplicationStartedEvent>
    {
        public const string NONE_PROFILE_NAME = "None";
        private readonly IMetadataProfileRepository _profileRepository;
        private readonly IArtistService _artistService;
        private readonly IImportListFactory _importListFactory;
        private readonly Logger _logger;

        public MetadataProfileService(IMetadataProfileRepository profileRepository,
                                      IArtistService artistService,
                                      IImportListFactory importListFactory,
                                      Logger logger)
        {
            _profileRepository = profileRepository;
            _artistService = artistService;
            _importListFactory = importListFactory;
            _logger = logger;
        }

        public MetadataProfile Add(MetadataProfile profile)
        {
            return _profileRepository.Insert(profile);
        }

        public void Update(MetadataProfile profile)
        {
            if (profile.Name == NONE_PROFILE_NAME)
            {
                throw new InvalidOperationException("Not permitted to alter None metadata profile");
            }

            _profileRepository.Update(profile);
        }

        public void Delete(int id)
        {
            var profile = _profileRepository.Get(id);

            if (profile.Name == NONE_PROFILE_NAME ||
                _artistService.GetAllArtists().Any(c => c.MetadataProfileId == id) ||
                _importListFactory.All().Any(c => c.MetadataProfileId == id))
            {
                throw new MetadataProfileInUseException(profile.Name);
            }

            _profileRepository.Delete(id);
        }

        public List<MetadataProfile> All()
        {
            return _profileRepository.All().ToList();
        }

        public MetadataProfile Get(int id)
        {
            return _profileRepository.Get(id);
        }

        public bool Exists(int id)
        {
            return _profileRepository.Exists(id);
        }

        private void AddDefaultProfile(string name, List<PrimaryAlbumType> primAllowed, List<SecondaryAlbumType> secAllowed, List<ReleaseStatus> relAllowed)
        {
            var primaryTypes = PrimaryAlbumType.All
                .OrderByDescending(l => l.Name)
                .Select(v => new ProfilePrimaryAlbumTypeItem {PrimaryAlbumType = v, Allowed = primAllowed.Contains(v)})
                .ToList();

            var secondaryTypes = SecondaryAlbumType.All
                .OrderByDescending(l => l.Name)
                .Select(v => new ProfileSecondaryAlbumTypeItem {SecondaryAlbumType = v, Allowed = secAllowed.Contains(v)})
                .ToList();

            var releaseStatues = ReleaseStatus.All
                .OrderByDescending(l => l.Name)
                .Select(v => new ProfileReleaseStatusItem {ReleaseStatus = v, Allowed = relAllowed.Contains(v)})
                .ToList();

            var profile = new MetadataProfile
            {
                Name = name,
                PrimaryAlbumTypes = primaryTypes,
                SecondaryAlbumTypes = secondaryTypes,
                ReleaseStatuses = releaseStatues
            };

            Add(profile);
        }

        public void Handle(ApplicationStartedEvent message)
        {
            var profiles = All();

            // Name is a unique property
            var emptyProfile = profiles.FirstOrDefault(x => x.Name == NONE_PROFILE_NAME);

            // make sure empty profile exists and is actually empty
            if (emptyProfile != null &&
                !emptyProfile.PrimaryAlbumTypes.Any(x => x.Allowed) &&
                !emptyProfile.SecondaryAlbumTypes.Any(x => x.Allowed) &&
                !emptyProfile.ReleaseStatuses.Any(x => x.Allowed))
            {
                return;
            }

            if (!profiles.Any())
            {
                _logger.Info("Setting up standard metadata profile");

                AddDefaultProfile("Standard", new List<PrimaryAlbumType>{PrimaryAlbumType.Album}, new List<SecondaryAlbumType>{ SecondaryAlbumType.Studio }, new List<ReleaseStatus>{ReleaseStatus.Official});
            }

            if (emptyProfile != null)
            {
                // emptyProfile is not the correct empty profile - move it out of the way
                _logger.Info($"Renaming non-empty metadata profile {emptyProfile.Name}");

                var names = profiles.Select(x => x.Name).ToList();

                int i = 1;
                emptyProfile.Name = $"{NONE_PROFILE_NAME}.{i}";

                while (names.Contains(emptyProfile.Name))
                {
                    i++;
                }

                _profileRepository.Update(emptyProfile);
            }

            _logger.Info("Setting up empty metadata profile");

            AddDefaultProfile(NONE_PROFILE_NAME, new List<PrimaryAlbumType>(), new List<SecondaryAlbumType>(), new List<ReleaseStatus>());
        }
    }
}
