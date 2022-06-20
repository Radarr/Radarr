using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration.Events;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Configuration
{
    public interface  IConfigFileWriter : IHandleAsync<ApplicationStartedEvent>,
        IExecute<ResetApiKeyCommand>
    {
        public void EnsureDefaultConfigFile();
        void SaveConfigDictionary(Dictionary<string, object> configValues);
    }

    public class ConfigFileWriter : IConfigFileWriter
    {
        public static string CONFIG_ELEMENT_NAME = BuildInfo.AppName;

        private readonly IEventAggregator _eventAggregator;
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigurationRoot _configuration;
        private readonly IOptionsMonitor<ConfigFileOptions> _configFileOptions;
        private readonly Logger _logger;

        private readonly string _configFile;

        private static readonly object Mutex = new object();

        public ConfigFileWriter(IAppFolderInfo appFolderInfo,
                                IEventAggregator eventAggregator,
                                IDiskProvider diskProvider,
                                IConfiguration configuration,
                                IOptionsMonitor<ConfigFileOptions> configFileOptions,
                                Logger logger)
        {
            _eventAggregator = eventAggregator;
            _diskProvider = diskProvider;
            _configuration = configuration as IConfigurationRoot;
            _configFileOptions = configFileOptions;
            _logger = logger;

            _configFile = appFolderInfo.GetConfigPath();

            _configFileOptions.OnChange(OnChange);
        }

        private Dictionary<string, object> GetConfigDictionary()
        {
            var dict = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

            var properties = typeof(ConfigFileOptions).GetProperties();

            foreach (var propertyInfo in properties)
            {
                var value = propertyInfo.GetValue(_configFileOptions.CurrentValue, null);

                dict.Add(propertyInfo.Name, value);
            }

            return dict;
        }

        public void SaveConfigDictionary(Dictionary<string, object> configValues)
        {
            var allWithDefaults = GetConfigDictionary();

            var persistKeys = typeof(ConfigFileOptions).GetProperties()
                .Where(x => Attribute.IsDefined(x, typeof(PersistAttribute)))
                .Select(x => x.Name)
                .ToList();

            foreach (var configValue in configValues)
            {
                if (configValue.Key.Equals("ApiKey", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                allWithDefaults.TryGetValue(configValue.Key, out var currentValue);
                if (currentValue == null)
                {
                    continue;
                }

                var equal = configValue.Value.ToString().Equals(currentValue.ToString());
                var persist = persistKeys.Contains(configValue.Key);

                if (persist || !equal)
                {
                    SetValue(configValue.Key.FirstCharToUpper(), configValue.Value.ToString());
                }
            }

            _eventAggregator.PublishEvent(new ConfigFileSavedEvent());
        }

        public void SetValue(string key, object value)
        {
            var valueString = value.ToString().Trim();
            var xDoc = LoadConfigFile();
            var config = xDoc.Descendants(CONFIG_ELEMENT_NAME).Single();

            var keyHolder = config.Descendants(key);

            if (keyHolder.Count() != 1)
            {
                config.Add(new XElement(key, valueString));
            }
            else
            {
                config.Descendants(key).Single().Value = valueString;
            }

            SaveConfigFile(xDoc);
        }

        public void EnsureDefaultConfigFile()
        {
            if (!File.Exists(_configFile))
            {
                SaveConfigDictionary(GetConfigDictionary());
                SetValue(nameof(ConfigFileOptions.ApiKey), _configFileOptions.CurrentValue.ApiKey);
            }
        }

        private void DeleteOldValues()
        {
            var xDoc = LoadConfigFile();
            var config = xDoc.Descendants(CONFIG_ELEMENT_NAME).Single();

            var properties = typeof(ConfigFileOptions).GetProperties();

            foreach (var configValue in config.Descendants().ToList())
            {
                var name = configValue.Name.LocalName;

                if (!properties.Any(p => p.Name == name))
                {
                    config.Descendants(name).Remove();
                }
            }

            SaveConfigFile(xDoc);
        }

        public XDocument LoadConfigFile()
        {
            try
            {
                lock (Mutex)
                {
                    if (_diskProvider.FileExists(_configFile))
                    {
                        var contents = _diskProvider.ReadAllText(_configFile);

                        if (contents.IsNullOrWhiteSpace())
                        {
                            throw new InvalidConfigFileException($"{_configFile} is empty. Please delete the config file and Radarr will recreate it.");
                        }

                        if (contents.All(char.IsControl))
                        {
                            throw new InvalidConfigFileException($"{_configFile} is corrupt. Please delete the config file and Radarr will recreate it.");
                        }

                        return XDocument.Parse(_diskProvider.ReadAllText(_configFile));
                    }

                    var xDoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
                    xDoc.Add(new XElement(CONFIG_ELEMENT_NAME));

                    return xDoc;
                }
            }
            catch (XmlException ex)
            {
                throw new InvalidConfigFileException($"{_configFile} is corrupt is invalid. Please delete the config file and Radarr will recreate it.", ex);
            }
        }

        private void SaveConfigFile(XDocument xDoc)
        {
            lock (Mutex)
            {
                _diskProvider.WriteAllText(_configFile, xDoc.ToString());
                _configuration.Reload();
            }
        }

        public void HandleAsync(ApplicationStartedEvent message)
        {
            DeleteOldValues();
        }

        public void Execute(ResetApiKeyCommand message)
        {
            SetValue(nameof(ConfigFileOptions.ApiKey), new ConfigFileOptions().ApiKey);
        }

        private void OnChange(ConfigFileOptions options)
        {
            _logger.Info("Config file updated");

            _eventAggregator.PublishEvent(new ConfigFileSavedEvent());
        }
    }
}
