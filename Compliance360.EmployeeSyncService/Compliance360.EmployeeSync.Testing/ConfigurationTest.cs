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

                Assert.AreEqual(job.Domain, "Domain");
                Assert.AreEqual(job.IntervalSeconds, 10);
                Assert.AreEqual(job.LdapQuery, "LdapQuery");
                Assert.AreEqual(job.Name, "Name");
                Assert.AreEqual(job.Ou, "OU");
                Assert.AreEqual(job.Password, "Password");
                Assert.AreEqual(job.RemoveGroupPrefix, "RemoveGroupPrefix");
                Assert.AreEqual(job.Type, "Type");
                
                Assert.Greater(job.Groups.Count, 0);

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