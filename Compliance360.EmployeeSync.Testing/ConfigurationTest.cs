using Compliance360.EmployeeSync.Library;
using Compliance360.EmployeeSync.Library.Configuration;
using NUnit.Framework;

namespace Compliance360.EmployeeSync.Testing
{
    [TestFixture]
    public class ConfigurationTest
    {
        [Test]
        public void ReadAllConfiguration()
        {
            var container = ContainerFactory.GetContainer();

            // get the list of jobs to process
            var configService = container.GetInstance<IConfigurationService>();
            var config = configService.GetConfig();

            Assert.Greater(config.Jobs.Count, 0, "Did not find any jobs in the configuration.");

            foreach (JobElement job in config.Jobs)
            {
                job.Domain = "Domain";
                job.IntervalSeconds = 10;
                job.LdapQuery = "LdapQuery";
                job.Name = "Name";
                job.Ou = "OU";
                job.Password = "Password";
                job.RemoveGroupPrefix = "RemoveGroupPrefix";
                job.Type = "Type";
                job.ErrorThreshold = 10;
                job.ErrorNotificationHost = "host";
                job.ErrorNotificationPort = 587;
                job.ErrorNotificationUseSsl = true;
                job.ErrorNotificationUserName = "username";
                job.ErrorNotificationPassword = "password";
                job.ErrorNotificationDomain = "domain";
                job.ErrorNotificationEmailFrom = "email_from";
                job.ErrorNotificationEmailTo = "email_to";
                job.ErrorNotificationSubject = "subject";

                Assert.AreEqual(job.Domain, "Domain");
                Assert.AreEqual(job.IntervalSeconds, 10);
                Assert.AreEqual(job.LdapQuery, "LdapQuery");
                Assert.AreEqual(job.Name, "Name");
                Assert.AreEqual(job.Ou, "OU");
                Assert.AreEqual(job.Password, "Password");
                Assert.AreEqual(job.RemoveGroupPrefix, "RemoveGroupPrefix");
                Assert.AreEqual(job.Type, "Type");
                Assert.Greater(job.Groups.Count, 0);
                Assert.AreEqual(job.ErrorThreshold, 10);
                Assert.AreEqual(job.ErrorNotificationHost, "host");
                Assert.AreEqual(job.ErrorNotificationPort, 587);
                Assert.AreEqual(job.ErrorNotificationUseSsl, true);
                Assert.AreEqual(job.ErrorNotificationUserName, "username");
                Assert.AreEqual(job.ErrorNotificationPassword, "password");
                Assert.AreEqual(job.ErrorNotificationDomain, "domain");
                Assert.AreEqual(job.ErrorNotificationEmailFrom, "email_from");
                Assert.AreEqual(job.ErrorNotificationEmailTo, "email_to");
                Assert.AreEqual(job.ErrorNotificationSubject, "subject");

                foreach (GroupElement grp in job.Groups)
                    Assert.True(!string.IsNullOrEmpty(grp.Name));

                foreach (AttributeElement attrib in job.Attributes)
                    Assert.True(!string.IsNullOrEmpty(attrib.Name));

                foreach (StreamElement stream in job.OutputStreams)
                {
                    Assert.True(!string.IsNullOrEmpty(stream.Name));

                    foreach (SettingElement setting in stream.Settings)
                    {
                        Assert.True(!string.IsNullOrEmpty(setting.Name));
                    }
                }
            }
        }
    }
}