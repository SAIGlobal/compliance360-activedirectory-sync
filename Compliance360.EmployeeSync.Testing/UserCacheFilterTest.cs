using System;
using Compliance360.EmployeeSync.Library;
using Compliance360.EmployeeSync.Library.Configuration;
using Compliance360.EmployeeSync.Library.Data;
using Compliance360.EmployeeSync.Library.Filters;
using NLog;
using NUnit.Framework;

namespace Compliance360.EmployeeSync.Testing
{
    /// <summary>
    ///     User cache filter tests
    /// </summary>
    [TestFixture]
    public class UserCacheFilterTest
    {
        [TearDown]
        public void Cleanup()
        {
            var logger = LogManager.GetCurrentClassLogger();
            var cacheService = new CacheService(logger, "TestJob");
            cacheService.DeleteCacheFile();
        }

        [Test]
        public void TestUserCacheFilter()
        {
            var logger = LogManager.CreateNullLogger();
            var cacheSvcFactory = new CacheServiceFactory();
            var cacheFilter = new UserCacheFilter(logger, cacheSvcFactory);

            // create the test user 
            var testUser = new ActiveDirectoryUser();
            testUser.Attributes[ActiveDirectoryService.AttributeWhenChanged] = DateTime.Now;
            testUser.Attributes[ActiveDirectoryService.AttributeDistinguishedName] = "cn=Thomas";
            testUser.Attributes[ActiveDirectoryService.AttributeDepartment] = "sales";

            var job = new JobElement
            {
                Name = "TestJob"
            };

            var cachedCheckedUser = cacheFilter.Execute(testUser, job);
            Assert.IsNotNull(cachedCheckedUser);

            // clean up the cache filter...this will cause the cache 
            // entries to be persisted to disk
            ((IDisposable) cacheFilter).Dispose();

            // validate that the cache entry is present
            // by trying to filter the user again
            cacheFilter = new UserCacheFilter(logger, cacheSvcFactory);

            cachedCheckedUser = cacheFilter.Execute(testUser, job);
            Assert.IsNull(cachedCheckedUser);

            // update a value on the user...should get the user 
            // since the hash should not match
            testUser.Attributes[ActiveDirectoryService.AttributeDepartment] = "development";
            cacheFilter = new UserCacheFilter(logger, cacheSvcFactory);
            cachedCheckedUser = cacheFilter.Execute(testUser, job);
            Assert.IsNotNull(cachedCheckedUser);
        }
    }
}