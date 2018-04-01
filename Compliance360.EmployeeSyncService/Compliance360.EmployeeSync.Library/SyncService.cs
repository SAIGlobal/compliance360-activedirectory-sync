using System;
using System.Collections.Generic;
using Compliance360.EmployeeSync.Library.Configuration;
using Compliance360.EmployeeSync.Library.Jobs;
using NLog;

namespace Compliance360.EmployeeSync.Library
{
    /// <summary>
    ///     This is the main service class. It drives the employee sychronization process
    /// </summary>
    public class SyncService : ISyncService
    {
        private readonly Stack<ISyncJob> _activeJobs = new Stack<ISyncJob>();

        public SyncService(IConfigurationService configService, ILogger logger)
        {
            ConfigService = configService;
            Logger = logger;
        }

        private ILogger Logger { get; }

        private IConfigurationService ConfigService { get; }

        /// <summary>
        ///     Starts the synchronization service process.
        /// </summary>
        public bool Start()
        {
            Logger.Debug("Begin Start()");

            var container = ContainerFactory.GetContainer();

            // get the list of jobs to process
            SyncConfigurationSection config;
            try
            {
                Logger.Debug("Loading configuration.");
                config = ConfigService.GetConfig();
            }
            catch (ConfigurationException ex)
            {
                Logger.Error(ex);
                throw;
            }

            foreach (JobElement jobConfig in config.Jobs)
            {
                // create the job based on the config
                ISyncJob job;
                try
                {
                    job = container.With("jobConfig").EqualTo(jobConfig)
                        .GetInstance<ISyncJob>(jobConfig.Type);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Problem loading job [{0}]", jobConfig.Name);
                    return false;
                }

                // try to start the service
                try
                {
                    job.Start();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Problem starting job [{0}]", jobConfig.Name);
                    return false;
                }

                _activeJobs.Push(job);
            }

            Logger.Debug("End Start()");
            return true;
        }

        /// <summary>
        ///     Stops the synchronization service process.
        /// </summary>
        public bool Stop()
        {
            Logger.Debug("Begin Stop()");

            while (_activeJobs.Count > 0)
            {
                using (var job = _activeJobs.Pop())
                {
                    try
                    {
                        job.Stop();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                }                
            }

            Logger.Debug("End Stop()");
            return true;
        }
    }
}