using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compliance360.EmployeeSync.Library.Data
{
    public interface ICacheService
    {
        void Add(string key, string value);
        bool ContainsKey(string key);
        void DeleteCacheFile();
        void WriteCacheEntries();
        string GetValue(string key);
    }
}
