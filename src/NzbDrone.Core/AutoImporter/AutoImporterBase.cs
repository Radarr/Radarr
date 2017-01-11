using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.AutoImporter
{
    public abstract class AutoImporterBase<TSettings> : IAutoImporter
        where TSettings : IProviderConfig, new()
    {
        // protected readonly IAutoImporterStatusService _autoImporterStatusService;
        protected readonly IConfigService _configService;
        protected readonly IParsingService _parsingService;
        protected readonly Logger _logger;

        public abstract string Name { get; }
        // public abstract DownloadProtocol Protocol { get; }
        public abstract string Link { get; }

        public abstract bool Enabled { get; }
        // public abstract bool SupportsSearch { get; }

        public AutoImporterBase(/*IAutoImporterStatusService autoImporterStatusService, */IConfigService configService, IParsingService parsingService, Logger logger)
        {
            //_autoImporterStatusService = autoImporterStatusService;
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

                yield return new AutoImporterDefinition
                {
                    Name = GetType().Name,
                    Link = Link,
                    Enabled = config.Validate().IsValid && Enabled,
                    Implementation = GetType().Name,
                    Settings = config
                };
            }
        }

        public virtual ProviderDefinition Definition { get; set; }

        public virtual object RequestAction(string action, IDictionary<string, string> query) { return null; }

        protected TSettings Settings => (TSettings)Definition.Settings;

        public abstract IList<ReleaseInfo> Fetch();

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
