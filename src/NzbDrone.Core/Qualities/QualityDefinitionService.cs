using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;
using System;
using System.Dynamic;
using System.Runtime.InteropServices;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Composition;
using NzbDrone.Common.Extensions;
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
        private IProfileService _profileService;
        private readonly IContainer _container;
        private readonly Logger _logger;

        public static IEnumerable<QualityDefinition> AllQualityDefinitions
        {
            get
            {
                if (_allQualityDefinitions == null)
                {
                    throw new Exception("***FATAL***: Tried accessing quality definitions before they were loaded. Please save this log and open an issue on github!");
                }

                return _allQualityDefinitions;
            }

            set
            {
                _allQualityDefinitions = value;
                AllQualityDefinitionsById = value.DistinctBy(d => d.Id).ToDictionary(d => d.Id);
                AllQualityDefinitionsByQuality = value.Where(d => d.Quality != null).ToDictionary(d => d.Quality); // We only want non custom formats here!

                // Note: Some cases might exist (e.g. tests) where the repo won't return anything, hence we don't have an unknown quality.
                // Accessing UnknownQualityDefinition will throw an error anyways.
                if (AllQualityDefinitionsByQuality.Keys.Contains(Quality.Unknown))
                {
                    _unknownQualityDefinition = AllQualityDefinitionsByQuality[Quality.Unknown].JsonClone();
                }

            }
        }

        public static IDictionary<int, QualityDefinition> AllQualityDefinitionsById;
        public static IDictionary<Quality, QualityDefinition> AllQualityDefinitionsByQuality;

        private static IEnumerable<QualityDefinition> _allQualityDefinitions;

        public static QualityDefinition UnknownQualityDefinition
        {
            get
            {
                if (AllQualityDefinitionsByQuality == null || _unknownQualityDefinition == null)
                {
                    throw new Exception("***FATAL***: Tried accessing quality definitions before they were loaded. Please save this log and open an issue on github!");
                }

                return _unknownQualityDefinition;
            }
        }

        private static QualityDefinition _unknownQualityDefinition;

        private static bool _applicationStarted = false;

        public QualityDefinitionService(IQualityDefinitionRepository repo, ICacheManager cacheManager,
            //IProfileService profileService,
            IContainer container,
            Logger logger)
        {
            _repo = repo;
            _cache = cacheManager.GetCache<IEnumerable<QualityDefinition>>(this.GetType());
            //_profileService = profileService;
            _container = container;
            _logger = logger;
        }

        private IEnumerable<QualityDefinition> GetAll()
        {
            //return QualityDefinition.DefaultQualityDefinitions.ToList().Select(WithWeight).ToDictionary(v => v.Quality);
            return _cache.Get("all", () =>
            {
                //Handle(new ApplicationStartedEvent()); //TODO: Update this horrible hack for integration tests.
                var all = _repo.All();
                var qualityDefinitions = all.ToList();
                all = qualityDefinitions.Select(d => WithParent(d, qualityDefinitions)).Select(WithWeight);
                AllQualityDefinitions = all;
                return all;
            }, TimeSpan.FromMinutes(15));
        }

        public void Update(QualityDefinition qualityDefinition)
        {
            _repo.Update(qualityDefinition);

            ClearCache();
        }

        public QualityDefinition Insert(QualityDefinition qualityDefinition)
        {
            var newQD = _repo.Insert(qualityDefinition);
            if (_profileService == null)
            {
                _profileService = _container.Resolve<IProfileService>();
            }
            _profileService.AddNewQuality(newQD);
            ClearCache();
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
            var existingDefinitions = _repo.All().Where(d => !d.ParentQualityDefinitionId.HasValue).ToList(); //Only get default definitions, not custom formats!

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

            ClearCache();
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
                ClearCache();
            }
        }

        private void ClearCache()
        {
            _cache.Clear();
            //AllQualityDefinitions = null; dont set them to null, else we have a race condition
            GetAll(); //Force cache to be refreshed, else AllQualityDefinitions won't get properly reset.
        }

        private static QualityDefinition WithWeight(QualityDefinition definition)
        {
            definition.Weight = QualityDefinition.DefaultQualityDefinitions.Single(d =>
                definition.ParentQualityDefinitionId != null
                    ? definition.ParentQualityDefinition.Quality == d.Quality
                    : d.Quality == definition.Quality).Weight; //Get weight from parent
            return definition;
        }

        private static QualityDefinition WithParent(QualityDefinition definition, IEnumerable<QualityDefinition> all)
        {
            definition.ParentQualityDefinition = all.FirstOrDefault(d => d.Id == definition.ParentQualityDefinitionId);
            return definition;
        }

        public void Handle(ApplicationStartedEvent message)
        {
            _logger.Debug("Setting up default quality config");

            InsertMissingDefinitions();

            //AddDefaultQualityTags();
        }
    }

    public class QualityWrapper : DynamicObject
    {
        public static readonly dynamic Dynamic = new QualityWrapper();

        public QualityDefinition GetPropertyValue(string propertyName)
        {
            var propInfo = typeof(Quality).GetProperty(propertyName);
            Quality quality = (Quality)propInfo?.GetValue(null, null);
            return quality != null ? QualityDefinitionService.AllQualityDefinitionsByQuality[quality] : null;
        }

        // Implement the TryGetMember method of the DynamicObject class for dynamic member calls.
        public override bool TryGetMember(GetMemberBinder binder,
            out object result)
        {
            result = GetPropertyValue(binder.Name);
            return result != null;
        }
    }
}
