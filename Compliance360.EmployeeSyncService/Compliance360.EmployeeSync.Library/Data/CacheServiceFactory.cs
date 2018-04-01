using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Compliance360.EmployeeSync.Library.Data
{
    public class CacheServiceFactory : ICacheServiceFactory
    {
        /// <summary>
        /// Creates a new cache service.
        /// </summary>
        /// <param name="logger">Reference to the logger</param>
        /// <param name="cacheName">Name of the cache to open or create</param>
        /// <param name="isMap">True, if the cache entries are bi-directional. (key-to-value and value-to-key)</param>
        /// <returns></returns>
        public ICacheService CreateCacheService(ILogger logger, string cacheName, bool isMap = false)
        {
            return new CacheService(logger, cacheName, isMap);
        }
    }
}
