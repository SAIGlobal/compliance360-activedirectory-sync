using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compliance360.EmployeeSync.Library.Configuration;
using Compliance360.EmployeeSync.Library.Data;
using Newtonsoft.Json;
using NLog;

namespace Compliance360.EmployeeSync.Library.OutputStreams
{
    public class LoggerStream : IOutputStream
    {
        private ILogger Logger { get; }
        private JobElement JobConfig { get; set; }
        private StreamElement StreamConfig { get; set; }

        /// <summary>
        /// Initializes a new instance of the LoggerStream
        /// </summary>
        /// <param name="logger">Instance of the logger.</param>
        /// <param name="jobConfig">The job configuration.</param>
        /// <param name="streamConfig">The stream configuration.</param>
        public LoggerStream(ILogger logger)
        {
            Logger = logger;
        }
        
        /// <summary>
        /// Closes the stream.
        /// </summary>
        public void Close()
        {
            Logger.Info("Closed the Logger Stream");
        }

        /// <summary>
        /// Opens the stream for writing.
        /// </summary>
        /// <param name="jobConfig">The current job configuration</param>
        /// <param name="streamConfig">The current stream configuration</param>
        public void Open(JobElement jobConfig, StreamElement streamConfig)
        {
            JobConfig = jobConfig;
            StreamConfig = streamConfig;
        }

        /// <summary>
        /// Writes the user to the stream
        /// </summary>
        /// <param name="user">Reference to the user to write to the stream.</param>
        public void Write(ActiveDirectoryUser user)
        {
            var json = JsonConvert.SerializeObject(user);            
            Logger.Info(json);
        }
    }
}
