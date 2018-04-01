using System.Configuration;
using System.Reflection;
using Compliance360.EmployeeSync.Library.Resources;
using NLog;

namespace Compliance360.EmployeeSync.Library.Configuration
{
    /// <summary>
    ///     This class is responsible for validating and providing
    ///     access to the sync service configuration.
    /// </summary>
    public class ConfigurationService : IConfigurationService
    {
        private const string SyncSectionName = "compliance360.sync";

        /// <summary>
        ///     Creates a new instance of the ConfigurationService
        /// </summary>
        /// <param name="logger">The logger reference</param>
        public ConfigurationService(ILogger logger)
        {
            Logger = logger;
        }

        /// <summary>
        ///     Reference to the class logger
        /// </summary>
        private ILogger Logger { get; }

        /// <summary>
        ///     Gets the current configuration settings for the Sync Service
        /// </summary>
        /// <exception cref="ConfigurationException">
        ///     Thrown if the configuration section is missing or if there is an error
        ///     loading the configuration.
        /// </exception>
        /// <returns>SyncConfigurationSection</returns>
        public SyncConfigurationSection GetConfig()
        {
            try
            {
                var config = (SyncConfigurationSection) ConfigurationManager.GetSection(SyncSectionName);
                if (config == null)
                {
                    var location = Assembly.GetAssembly(typeof(ConfigurationService)).Location;
                    throw new ConfigurationException(ResourceManager.GetString("ErrorConfigurationSectionMissing",
                        location));
                }

                return config;
            }
            catch (ConfigurationErrorsException ex)
            {
                Logger.Error(ex);
                throw new ConfigurationException(ex.Message, ex);
            }
        }
    }
}