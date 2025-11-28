using System;
using Microsoft.Extensions.Configuration;

namespace MyLab.Task.Runtime
{
    class TaskConfigBuilder
    {
        private IConfiguration _mainConfig;
        private RuntimeOptions _options;

        public TaskConfigBuilder(IConfiguration mainConfig, RuntimeOptions options)
        {
            _mainConfig = mainConfig ?? throw new ArgumentNullException(nameof(mainConfig));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public static TaskConfigBuilder FromConfig(IConfiguration mainConfig, string appConfigSectionName)
        {
            if (mainConfig is null) throw new ArgumentNullException(nameof(mainConfig));
            if (string.IsNullOrEmpty(appConfigSectionName)) throw new ArgumentException($"'{nameof(appConfigSectionName)}' cannot be null or empty.", nameof(appConfigSectionName));

            var appSection = mainConfig.GetSection(appConfigSectionName);

            var options = new RuntimeOptions();
            appSection.Bind(options);

            return new TaskConfigBuilder(mainConfig, options);
        }

        public IConfiguration Build(string taskName)
        {
            var baseConfigBuilder = new ConfigurationBuilder();

            baseConfigBuilder.AddSubconfig(_mainConfig.GetSection("Logging"), "Logging");

            if(_options.BaseTaskConfig != null)
            {
                baseConfigBuilder.AddConfiguration(_options.BaseTaskConfig);
            }

            if
            (
                _options.Tasks != null && 
                _options.Tasks.TryGetValue(taskName, out var tOpt) &&
                tOpt.Config != null
            )
            {
                baseConfigBuilder.AddConfiguration(tOpt.Config);
            }

            return baseConfigBuilder.Build();
        }
    }
}
