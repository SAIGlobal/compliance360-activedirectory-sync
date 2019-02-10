using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compliance360.EmployeeSync.Library.Configuration;

namespace Compliance360.EmployeeSync.Library.Notifications
{
    public interface IEmailNotificationService
    {
        /// <summary>
        /// Sends an email notification message.
        /// </summary>
        /// <param name="jobConfig">The job config that contains the SMTP server configuration.</param>
        /// <param name="message">The message to send.</param>
        void SendEmailNotification(JobElement jobConfig, string message);
    }
}
