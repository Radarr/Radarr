using System;
using System.Collections.Generic;
using FluentValidation.Results;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.NetImport
{
    public class NetImportFetchResult
    {
        public IList<Movie> Movies { get; set; }
        public bool AnyFailure { get; set; }
    }

    public abstract class NetImportBase<TSettings> : INetImport
        where TSettings : IProviderConfig, new()
    {
        protected readonly INetImportStatusService _netImportStatusService;
        protected readonly IConfigService _configService;
        protected readonly IParsingService _parsingService;
        protected readonly Logger _logger;

        public abstract string Name { get; }

        public abstract NetImportType ListType { get; }
        public abstract bool Enabled { get; }
        public abstract bool EnableAuto { get; }

        public abstract NetImportFetchResult Fetch();

        protected NetImportBase(INetImportStatusService netImportStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
        {
            _netImportStatusService = netImportStatusService;
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

                yield return new NetImportDefinition
                {
                    Name = GetType().Name,
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
