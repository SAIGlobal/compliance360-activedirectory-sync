using System;
using System.IO;
using Compliance360.EmployeeSync.Library.Configuration;
using NLog;
using Rebex;
using Rebex.Net;

namespace Compliance360.EmployeeSync.Library.Data
{
    public class SftpService : ISftpService
    {
        /// <summary>
        ///     Initializes a new instance of the SftpService
        /// </summary>
        public SftpService(ILogger logger)
        {
            Logger = logger;
        }

        private ILogger Logger { get; }

        /// <summary>
        ///     Sends the user json file to an sftp server
        ///     defined in the job configuration.
        /// </summary>
        /// <param name="filePath">The path to the file to transfer.</param>
        /// <param name="host">Host name or IP Address.</param>
        /// <param name="userName">The username used to connect to the sftp server.</param>
        /// <param name="password">The password used to connect to the sftp server.</param>
        /// <param name="destinationPath">The destination path on the sftp server.</param>
        public void SendFile(string filePath, 
            string host,
            string userName,
            string password,
            string destinationPath)
        {
            Sftp sftp;
            try
            {
#if DEBUG
                Licensing.Key = "==AfWaAqXO6tGC1ijhm0kMs02eGtJaIJwRfIYEoq/uEPXA==";
#endif
                Logger.Debug("Connecting to host: {0}@{1}", userName, host);
                sftp = new Sftp();
                sftp.Connect(host);
                sftp.Login(userName, password);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error connecting to [{0}] with User=[{1}] Password=[hidden].",
                    host,
                    userName);
                throw;
            }

            try
            {
                // build the destination file path
                var fileName = Path.GetFileName(filePath);
                if (destinationPath[destinationPath.Length - 1] != '/')
                    destinationPath += '/';
                var prefix = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss_");
                destinationPath += prefix + fileName;

                // send the file
                Logger.Debug("Sending file: [{0}] to [{1}]", filePath, destinationPath);
                sftp.PutFile(filePath, destinationPath);

                // disconnect from the server
                Logger.Debug("Disconnecting from sftp server.");
                sftp.Disconnect();
                sftp.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error sending file [{0}] to [{1}]", filePath, destinationPath);
                throw;
            }
        }
    }
}