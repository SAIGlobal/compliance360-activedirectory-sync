using System;
using System.IO;
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
    public class SftpStream : IOutputStream
    {
        private ILogger Logger { get; }
        private JobElement JobConfig { get; set; }
        private StreamElement StreamConfig { get; set; }
        private StreamWriter DataStream { get; set;  }
        private ISftpService SftpService { get; }
        private bool HasUsers { get; set; }
        private string FileName { get; set; }

        /// <summary>
        /// Initializes a new instance of the SftpStream
        /// </summary>
        /// <param name="logger">Instance of the logger.</param>
        public SftpStream(
            ILogger logger, 
            ISftpService sftpService)
        {
            Logger = logger;
            SftpService = sftpService;
        }

        /// <summary>
        /// Closes the stream. We used the close method to transfer
        /// the file via ftp.
        /// </summary>
        public void Close()
        {
            DataStream.WriteLine("]}");
            DataStream.Flush();
            DataStream.Close();

            // transfer the file to the sftp service
            if (!HasUsers)
            {
                Logger.Debug("No Users to update. Skipping sftp process.");
            }
            else
            {
                var host = StreamConfig.Settings["host"];
                var username = StreamConfig.Settings["username"];
                var password = StreamConfig.Settings["password"];
                var destinationPath = StreamConfig.Settings["destinationPath"];

                SftpService.SendFile(FileName, host, username, password, destinationPath);
            }

            // clean up the temp file
            try
            {
                File.Delete(FileName);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Opens the stream for writing
        /// </summary>
        /// <param name="jobConfig">The current job configuration</param>
        /// <param name="streamConfig">The current stream configuration</param>
        public void Open(JobElement jobConfig, StreamElement streamConfig)
        {
            JobConfig = jobConfig;
            StreamConfig = streamConfig;

            // build the time name based on 
            var safeJobName = GetSafeFileName(JobConfig.Name);
            FileName = $"{Path.GetTempPath()}{safeJobName}";

            // open a temp file to stream the user data
            Logger.Debug("Writting users to file: \"{0}\"", FileName);
            DataStream = new StreamWriter(FileName);
            DataStream.WriteLine("{\n\"users\":[");
            
        }
        /// <summary>
        /// Writes the user to the stream
        /// </summary>
        /// <param name="user">Reference to the user to write to the stream.</param>
        public void Write(ActiveDirectoryUser user)
        {
            if (!HasUsers)
            {
                HasUsers = true;
            }
            else
            {
                DataStream.WriteLine(",\n");
            }
    
            // serialize the object to json and write to the file
            var userJson = JsonConvert.SerializeObject(user);
            DataStream.Write(userJson);
        }

        /// <summary>
        ///     Builds a safe file name
        /// </summary>
        /// <param name="jobName"></param>
        private string GetSafeFileName(string jobName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var cleanFileName = new string(jobName.Where(m => !invalidChars.Contains(m)).ToArray()) + ".json";
            return cleanFileName;
        }
    }
}
