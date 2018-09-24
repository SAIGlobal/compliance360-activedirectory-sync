using System;
using System.Collections.Generic;
using System.Linq;
using Compliance360.EmployeeSync.Library;
using Compliance360.EmployeeSync.Library.Configuration;
using Compliance360.EmployeeSync.Library.Data;
using Compliance360.EmployeeSync.Library.Filters;
using NLog;
using NUnit.Framework;

namespace Compliance360.EmployeeSync.Testing
{
    [TestFixture]
    public class ActiveDirectoryServiceTests
    {
        [Test]
        public void TestBuildActiveDirectoryUser()
        {
            var logger = ContainerFactory.GetContainer().GetInstance<ILogger>();
            var config = ContainerFactory.GetContainer().GetInstance<IConfigurationService>().GetConfig();
            var jobConfig = config.Jobs[0];
            var userFilterFactory = new UserFilterFactory();
            var attributeFilterFactory = new AttributeFilterFactory();
            var service = new ActiveDirectoryService(logger, userFilterFactory, attributeFilterFactory);

            var searchResult = SearchResultFactory.Construct(new
            {
                sAMAccountName = "leetho0",
                manager = "Mike Gilbert",
                department = "development",
                givenName = "Thomas",
                sn = "Lee"
            });

            var filters = new Dictionary<string, IAttributeFilter>();

            var user = service.BuildActiveDirectoryUser(searchResult, jobConfig, filters);
            Assert.AreEqual(user.Attributes["sAMAccountName"], "leetho0");
        }

        [Test]
        public void TestGetAttributeFilter()
        {
            var logger = ContainerFactory.GetContainer().GetInstance<ILogger>();
            var userFilterFactory = new UserFilterFactory();
            var attributeFilterFactory = new AttributeFilterFactory();
            var service = new ActiveDirectoryService(logger, userFilterFactory, attributeFilterFactory);
            var filters = new Dictionary<string, IAttributeFilter>();
            var filter = service.GetAttributeFilter("ReadLdapFilter", filters);
            Assert.IsNotNull(filter);
        }

        [Test]
        public void TestGetAttributeFilterCached()
        {
            var logger = ContainerFactory.GetContainer().GetInstance<ILogger>();
            var userFilterFactory = new UserFilterFactory();
            var attributeFilterFactory = new AttributeFilterFactory();
            var service = new ActiveDirectoryService(logger, userFilterFactory, attributeFilterFactory);
            var filters = new Dictionary<string, IAttributeFilter>();
            var filter = service.GetAttributeFilter("ReadLdapFilter", filters);
            Assert.IsNotNull(filter);

            filter = service.GetAttributeFilter("ReadLdapFilter", filters);
            Assert.IsNotNull(filter);
        }

        [Test]
        public void TestGetAttributeFilterMissing()
        {
            var logger = ContainerFactory.GetContainer().GetInstance<ILogger>();
            var userFilterFactory = new UserFilterFactory();
            var attributeFilterFactory = new AttributeFilterFactory();
            var service = new ActiveDirectoryService(logger, userFilterFactory, attributeFilterFactory);
            try
            {
                var filters = new Dictionary<string, IAttributeFilter>();
                service.GetAttributeFilter("Missing filter", filters);
                Assert.Fail();
            }
            catch (Exception)
            {
                Assert.Pass();
            }
        }

        [Test]
        public void TestGetBaseLdapAddressDomain()
        {
            var address = ActiveDirectoryService.GetBaseLdapAddress("test.domain.com", null);
            Assert.AreEqual(address, "LDAP://DC=test,DC=domain,DC=com");
        }

        [Test]
        public void TestGetBaseLdapAddressDomainAndOu()
        {
            var address = ActiveDirectoryService.GetBaseLdapAddress("test.domain.com", "OU=corporate");
            Assert.AreEqual(address, "LDAP://OU=corporate,DC=test,DC=domain,DC=com");
        }

        [Test]
        public void TestGetPropertyNamesForJobs()
        {
            var config = new ConfigurationService(null).GetConfig();
            var names = ActiveDirectoryService.GetPropertyNamesForJob(config.Jobs[0].Attributes);
            Assert.Greater(names.Length, 0);
            Assert.True(names.Contains("sAMAccountName"));
        }
    }
}