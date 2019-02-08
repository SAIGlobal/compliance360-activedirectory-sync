using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Compliance360.EmployeeSync.Library.Configuration;

namespace Compliance360.EmployeeSync.Library.Notifications
{
    public class EmailNotificationService
    {
        public void SendEmailNotification(JobElement jobConfig, string message)
        {

            MailMessage mail = new MailMessage(jobConfig.ErrorNotificationEmailFrom, jobConfig.ErrorNotificationEmailTo);
            SmtpClient client = new SmtpClient();
            client.Port = jobConfig.ErrorNotificationPort;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Host = jobConfig.ErrorNotificationHost;
            client.EnableSsl = true;
            if (!string.IsNullOrEmpty(jobConfig.ErrorNotificationUserName))
            {
                if (!string.IsNullOrEmpty(jobConfig.ErrorNotificationDomain))
                {
                    client.Credentials = new NetworkCredential(jobConfig.ErrorNotificationUserName, jobConfig.ErrorNotificationPassword, jobConfig.ErrorNotificationDomain);
                }
                else
                {
                    client.Credentials = new NetworkCredential(jobConfig.ErrorNotificationUserName, jobConfig.ErrorNotificationPassword);
                }
            }
            
            mail.Subject = jobConfig.ErrorNotificationSubject;
            mail.Body = message;
            client.Send(mail);
        }
    }
}
