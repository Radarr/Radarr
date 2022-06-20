using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Update
{
    public interface IUpdaterConfigProvider
    {
    }

    public class UpdaterConfigProvider : IUpdaterConfigProvider, IHandle<ApplicationStartedEvent>
    {
        private readonly Logger _logger;
        private readonly IOptionsMonitor<ConfigFileOptions> _configFileProvider;
        private readonly IConfigFileWriter _configFileWriter;
        private readonly IDeploymentInfoProvider _deploymentInfoProvider;

        public UpdaterConfigProvider(IDeploymentInfoProvider deploymentInfoProvider, IOptionsMonitor<ConfigFileOptions> configFileProvider, IConfigFileWriter configFileWriter, Logger logger)
        {
            _deploymentInfoProvider = deploymentInfoProvider;
            _configFileProvider = configFileProvider;
            _configFileWriter = configFileWriter;
            _logger = logger;
        }

        public void Handle(ApplicationStartedEvent message)
        {
            var updateMechanism = _configFileProvider.CurrentValue.UpdateMechanism;
            var packageUpdateMechanism = _deploymentInfoProvider.PackageUpdateMechanism;

            var externalMechanisms = Enum.GetValues(typeof(UpdateMechanism))
                                         .Cast<UpdateMechanism>()
                                         .Where(v => v >= UpdateMechanism.External)
                                         .ToArray();

            foreach (var externalMechanism in externalMechanisms)
            {
                if ((packageUpdateMechanism != externalMechanism && updateMechanism == externalMechanism) ||
                    (packageUpdateMechanism == externalMechanism && updateMechanism == UpdateMechanism.BuiltIn))
                {
                    _logger.Info("Update mechanism {0} not supported in the current configuration, changing to {1}.", updateMechanism, packageUpdateMechanism);
                    ChangeUpdateMechanism(packageUpdateMechanism);
                    break;
                }
            }

            if (_deploymentInfoProvider.IsExternalUpdateMechanism)
            {
                var currentBranch = _configFileProvider.CurrentValue.Branch;
                var packageBranch = _deploymentInfoProvider.PackageBranch;
                if (packageBranch.IsNotNullOrWhiteSpace() && packageBranch != currentBranch)
                {
                    _logger.Info("External updater uses branch {0} instead of the currently selected {1}, changing to {0}.", packageBranch, currentBranch);
                    ChangeBranch(packageBranch);
                }
            }
        }

        private void ChangeUpdateMechanism(UpdateMechanism updateMechanism)
        {
            var config = new Dictionary<string, object>
            {
                [nameof(_configFileProvider.CurrentValue.UpdateMechanism)] = updateMechanism
            };
            _configFileWriter.SaveConfigDictionary(config);
        }

        private void ChangeBranch(string branch)
        {
            var config = new Dictionary<string, object>
            {
                [nameof(_configFileProvider.CurrentValue.Branch)] = branch
            };
            _configFileWriter.SaveConfigDictionary(config);
        }
    }
}
