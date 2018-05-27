using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compliance360.EmployeeSync.Library.Data;
using NLog;
using NUnit.Framework;

namespace Compliance360.EmployeeSync.Testing
{
    [TestFixture]
    public class CacheServiceTests
    {
        [Test]
        public void TestReadWriteNoMapCache()
        {
            var logger = LogManager.GetCurrentClassLogger();
            var cache = new CacheService(logger, "TEST_CACHE", false);
            var key = "THE_KEY";
            var value = "THE_VALUE";
            cache.Add(key, value);

            // key should be present
            Assert.IsTrue(cache.ContainsKey(key));

            // should be able to get the value
            Assert.AreEqual(value, cache.GetValue(key));
        }

        [Test]
        public void TestReadWriteMapCache()
        {
            var logger = LogManager.GetCurrentClassLogger();
            var cache = new CacheService(logger, "TEST_CACHE", true);
            var key = "THE_KEY";
            var value = "THE_VALUE";
            cache.Add(key, value);

            // key and value should be present as keys
            Assert.IsTrue(cache.ContainsKey(key));
            Assert.IsTrue(cache.ContainsKey(value));

            // should be able to get the values
            Assert.AreEqual(value, cache.GetValue(key));
            Assert.AreEqual(key, cache.GetValue(value));
        }

        [Test]
        public void TestPersistCache()
        {
            var logger = LogManager.GetCurrentClassLogger();
            var cache = new CacheService(logger, "TEST_CACHE", true);
            var key = "THE_KEY";
            var value = "THE_VALUE";
            cache.Add(key, value);

            cache.WriteCacheEntries();

            // load the cache again...should read it from the file
            cache = new CacheService(logger, "TEST_CACHE", true);
            Assert.AreEqual(value, cache.GetValue(key));

            // delete the cache file
            cache.DeleteCacheFile();

            //load it again and ensure it is empty
            cache = new CacheService(logger, "TEST_CACHE", true);
            Assert.IsFalse(cache.ContainsKey(key));
        }
    }
}
