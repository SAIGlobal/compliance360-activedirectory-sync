using System;
using System.IO;
using System.Linq;
using Compliance360.EmployeeSync.Library.Configuration;
using Compliance360.EmployeeSync.Library.Data;
using Compliance360.EmployeeSync.Library.OutputStreams;
using Newtonsoft.Json;
using NLog;
using System.Collections.Generic;

namespace Compliance360.EmployeeSync.Library.Jobs
{
    /// <summary>
    ///     This job will extract AD information and send it to
    ///     a designated sftp location
    /// </summary>
    public class ActiveDirectorySyncJob : SyncJob
    {
        public ActiveDirectorySyncJob(ILogger logger,
            JobElement jobConfig,
            IActiveDirectoryService activeDirectoryService,
            IOutputStreamFactory outputStreamFactory) : base(logger, jobConfig)
        {
            ActiveDirectoryService = activeDirectoryService;
            OutputStreamFactory = outputStreamFactory;
        }
        
        private IActiveDirectoryService ActiveDirectoryService { get; }
        
        private IOutputStreamFactory OutputStreamFactory { get; }
    
        /// <summary>
        /// Performs the active directory synchronization activities
        /// </summary>
        protected override void Execute()
        {
            Logger.Debug("Start ActiveDirectorySyncJob.Execute()");

            Logger.Info("Starting processing for job: {0}", JobConfig.Name);

            // ensure that the config contains at least one output stream
            if (this.JobConfig.OutputStreams.Count == 0)
            {
                throw new ConfigurationException($"Job {JobConfig.Name} must have at least one <outputStream> defined.");
            }

            // load the streams for the job
            var streams = new List<IOutputStream>();
            foreach (StreamElement streamConfig in this.JobConfig.OutputStreams)
            {
                var stream = OutputStreamFactory.CreateOutputStream(JobConfig, streamConfig);
                streams.Add(stream);
            }

            // get the users from active directory based on the job configuration
            try
            {
                var users = ActiveDirectoryService.GetActiveDirectoryUsers(JobConfig);
                foreach (var user in users)
                {
                    // write the user to each stream in the list
                    foreach (var stream in streams)
                    {
                        try
                        {
                            stream.Write(user);
                        }
                        catch (Exception ex)
                        {
                            // catch exceptions at the user level
                            // and log them, then keep going since we do not
                            // want to stop processing other users
                            Logger.Error(ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            // tell each of the streams that process is complete
            // this allows for any post processing behavior
            foreach (var stream in streams)
            {
                try
                {
                    stream.StreamComplete();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

            // close each of the streams
            foreach (var stream in streams)
            {
                try
                {
                    stream.Close();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

            Logger.Debug("End ActiveDirectorySyncJob.Execute()");
        }
    }
}