using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.ImportLists.ImportListMovies;
using NzbDrone.Core.Parser;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.ImportLists
{
    public class ImportListFetchResult
    {
        public ImportListFetchResult()
        {
            Movies = new List<ImportListMovie>();
        }

        public List<ImportListMovie> Movies { get; set; }
        public bool AnyFailure { get; set; }
        public int SyncedLists { get; set; }
    }

    public abstract class ImportListBase<TSettings> : IImportList
        where TSettings : IProviderConfig, new()
    {
        protected readonly IImportListStatusService _importListStatusService;
        protected readonly IConfigService _configService;
        protected readonly IParsingService _parsingService;
        protected readonly Logger _logger;

        public abstract string Name { get; }

        public abstract ImportListType ListType { get; }
        public abstract TimeSpan MinRefreshInterval { get; }
        public abstract bool Enabled { get; }
        public abstract bool EnableAuto { get; }

        public abstract ImportListFetchResult Fetch();

        protected ImportListBase(IImportListStatusService importListStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
        {
            _importListStatusService = importListStatusService;
            _configService = configService;
            _parsingService = parsingService;
            _logger = logger;
        }

        public Type ConfigContract => typeof(TSettings);

        public virtual ProviderMessage Message => null;

        public virtual IEnumerable<ProviderDefinition> DefaultDefinitions
        {
            get
            {
                var config = (IProviderConfig)new TSettings();

                yield return new ImportListDefinition
                {
                    Enabled = config.Validate().IsValid && Enabled,
                    EnableAuto = true,
                    Implementation = GetType().Name,
                    Settings = config
                };
            }
        }

        public virtual ProviderDefinition Definition { get; set; }

        public virtual object RequestAction(string action, IDictionary<string, string> query)
        {
            return null;
        }

        protected virtual List<ImportListMovie> CleanupListItems(IEnumerable<ImportListMovie> listMovies)
        {
            var result = listMovies.ToList();

            result.ForEach(c =>
            {
                c.ListId = Definition.Id;
            });

            return result;
        }

        protected TSettings Settings => (TSettings)Definition.Settings;

        public ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            try
            {
                Test(failures);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Test aborted due to exception");
                failures.Add(new ValidationFailure(string.Empty, "Test was aborted due to an error: " + ex.Message));
            }

            return new ValidationResult(failures);
        }

        protected abstract void Test(List<ValidationFailure> failures);

        public override string ToString()
        {
            return Definition.Name;
        }
    }
}
