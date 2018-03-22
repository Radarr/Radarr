using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;
using System;
using System.Runtime.InteropServices;
using NzbDrone.Common.Cache;
using NzbDrone.Core.Profiles;

namespace NzbDrone.Core.Qualities
{
    public interface IQualityDefinitionService
    {
        void Update(QualityDefinition qualityDefinition);
        QualityDefinition Insert(QualityDefinition qualityDefinition);
        List<QualityDefinition> All();
        QualityDefinition GetById(int id);
        QualityDefinition Get(Quality quality);
    }

    public class QualityDefinitionService : IQualityDefinitionService, IHandle<ApplicationStartedEvent>
    {
        private readonly IQualityDefinitionRepository _repo;
        private readonly ICached<IEnumerable<QualityDefinition>> _cache;
        private readonly IProfileService _profileService;
        private readonly Logger _logger;

        public QualityDefinitionService(IQualityDefinitionRepository repo, ICacheManager cacheManager,
            //IProfileService profileService,
            Logger logger)
        {
            _repo = repo;
            _cache = cacheManager.GetCache<IEnumerable<QualityDefinition>>(this.GetType());
            //_profileService = profileService;
            _logger = logger;
        }

        private IEnumerable<QualityDefinition> GetAll()
        {
            //return QualityDefinition.DefaultQualityDefinitions.ToList().Select(WithWeight).ToDictionary(v => v.Quality);
            return _cache.Get("all", () => _repo.All().Select(WithWeight), TimeSpan.FromSeconds(5.0));
        }

        public void Update(QualityDefinition qualityDefinition)
        {
            _repo.Update(qualityDefinition);

            _cache.Clear();
        }

        public QualityDefinition Insert(QualityDefinition qualityDefinition)
        {
            var newQD = _repo.Insert(qualityDefinition);
            //TODO: actually use this once profile is updated. _profileService.AddNewQuality(newQD);
            return newQD;
        }

        public List<QualityDefinition> All()
        {
            return GetAll().OrderBy(d => d.Weight).ToList();
        }

        public QualityDefinition GetById(int id)
        {
            return GetAll().Single(v => v.Id == id);
        }

        public QualityDefinition Get(Quality quality)
        {
            return GetAll().First(v => v.Quality == quality);
        }

        private void InsertMissingDefinitions()
        {
            List<QualityDefinition> insertList = new List<QualityDefinition>();
            List<QualityDefinition> updateList = new List<QualityDefinition>();

            var allDefinitions = QualityDefinition.DefaultQualityDefinitions.OrderBy(d => d.Weight).ToList();
            var existingDefinitions = _repo.All().ToList();

            foreach (var definition in allDefinitions)
            {
                var existing = existingDefinitions.SingleOrDefault(d => d.Quality == definition.Quality);

                if (existing == null)
                {
                    insertList.Add(definition);
                }

                else
                {
                    updateList.Add(existing);
                    existingDefinitions.Remove(existing);
                }
            }

            _repo.InsertMany(insertList);
            _repo.UpdateMany(updateList);
            _repo.DeleteMany(existingDefinitions);

            _cache.Clear();
        }

        private void AddDefaultQualityTags()
        {
            var allDefinitions = All();
            if (!allDefinitions.Any(d => d.QualityTags != null && d.QualityTags.Count > 0))
            {
                _logger.Debug("Adding default quality tags, since none are in the repo");
                var defaults = QualityDefinition.DefaultQualityDefinitions;
                var updateList = new List<QualityDefinition>();
                foreach (var definition in allDefinitions)
                {
                    definition.QualityTags = defaults.Single(d => d.Quality == definition.Quality).QualityTags;
                    updateList.Add(definition);
                }
                _repo.UpdateMany(updateList);
                _cache.Clear();
            }
        }

        private static QualityDefinition WithWeight(QualityDefinition definition)
        {
            definition.Weight = QualityDefinition.DefaultQualityDefinitions.Single(d =>
                definition.ParentQualityDefinitionId != null
                    ? definition.ParentQualityDefinitionId.Value == d.Id
                    : d.Quality == definition.Quality).Weight; //Get weight from parent
            return definition;
        }

        public void Handle(ApplicationStartedEvent message)
        {
            _logger.Debug("Setting up default quality config");

            InsertMissingDefinitions();

            AddDefaultQualityTags();
        }
    }
}
