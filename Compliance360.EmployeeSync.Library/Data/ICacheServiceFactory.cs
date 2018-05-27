using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Compliance360.EmployeeSync.Library.Data
{
    public interface ICacheServiceFactory
    {
        ICacheService CreateCacheService(ILogger logger, string cacheName, bool isMap = false);
    }
}
