using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using NLog;

namespace Compliance360.EmployeeSync.Library.Data
{
    public class CacheService : ICacheService
    {
        private readonly Dictionary<string, string> _cache = new Dictionary<string, string>();
        private readonly string _cacheName;
        private readonly bool _isMap;

        /// <summary>
        /// Initializes a new instance of the CacheService
        /// </summary>
        /// <param name="logger">The current logger instance</param>
        /// <param name="cacheName">The name of the cache</param>
        public CacheService(ILogger logger, string cacheName, bool isMap = false)
        {
            Logger = logger;
            _cacheName = cacheName;
            _isMap = isMap;

            // load the cache if the file is already present
            LoadCacheEntries();
        }

        private ILogger Logger { get; }

        /// <summary>
        /// Adds an item to the cache
        /// </summary>
        /// <param name="key">Key of the cache entry.</param>
        /// <param name="value">Value of the cache entry.</param>
        public void Add(string key, string value)
        {
            _cache[key] = value;

            if (_isMap)
            {
                _cache[value] = key;
            }
        }

        /// <summary>
        /// Checks to see if a key exists.
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <returns>True, if the key exissts in the cache.</returns>
        public bool ContainsKey(string key)
        {
            var val = _cache.ContainsKey(key);

            if (val)
            {
                Logger.Debug("Cache [{0}] - Hit for key {1}", _cacheName, key);
            }
            else
            {
                Logger.Debug("Cache [{0}] - Miss for key {1}", _cacheName, key);
            }

            return val;
        }

        /// <summary>
        ///     Deletes a user cache file by Job Name
        /// </summary>
        public void DeleteCacheFile()
        {
            var fileName = GetCacheFileName(_cacheName);

            using (var store = IsolatedStorageFile.GetMachineStoreForDomain())
            {
                if (!store.FileExists(fileName))
                    return;

                store.DeleteFile(fileName);
            }
        }

        /// <summary>
        /// Gets a value from the cache.
        /// </summary>
        /// <param name="key">Key to the item in the cache.</param>
        /// <returns>Cache value or null if not found.</returns>
        public string GetValue(string key)
        {
            if (_cache.ContainsKey(key))
            {
                return _cache[key];
            }

            return null;
        }

        /// <summary>
        /// Removes the specified key from the cache
        /// </summary>
        /// <param name="key">Key of the item to remove</param>
        public void Remove(string key)
        {
            if (_cache.ContainsKey(key))
            {
                _cache.Remove(key);
            }
        }

        /// <summary>
        ///     Writes the current cache entries to the file.
        /// </summary>
        public void WriteCacheEntries()
        {
            if (_cache.Count == 0)
                return;

            var fileName = GetCacheFileName(_cacheName);

            using (var store = IsolatedStorageFile.GetMachineStoreForDomain())
            {
                using (var cacheFile = store.OpenFile(fileName, FileMode.Create, FileAccess.Write))
                {
                    try
                    {
                        // Retrieve the actual path of the file using reflection.
                        var path = cacheFile.GetType()
                            .GetField("m_FullPath", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(cacheFile)
                            .ToString();
                        Logger.Debug("Writing cache entries to file: {0}", path);
                    }
                    catch (Exception)
                    {
                       
                    }
                    

                    cacheFile.Position = 0;
                    var writer = new StreamWriter(cacheFile);
                    foreach (var key in _cache.Keys)
                    {
                        writer.WriteLine($"{key}||{_cache[key]}");
                    }
                    writer.Flush();
                    writer.Close();
                }
            }
        }

        /// <summary>
        ///     Builds the cache file name
        /// </summary>
        /// <param name="jobName"></param>
        private string GetCacheFileName(string jobName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var cleanFileName = new string(jobName.Where(m => !invalidChars.Contains(m)).ToArray()) + ".txt";
            return cleanFileName;
        }

        /// <summary>
        ///     Populates the cache entries
        /// </summary>
        private void LoadCacheEntries()
        {
            var fileName = GetCacheFileName(_cacheName);

            using (var store = IsolatedStorageFile.GetMachineStoreForDomain())
            {
                if (!store.FileExists(fileName))
                    return;

                using (var cacheFile = store.OpenFile(fileName, FileMode.OpenOrCreate, FileAccess.Read))
                {
                    Logger.Debug("Loading cache file: {0}", cacheFile.Name);

                    var streamReader = new StreamReader(cacheFile);
                    while (!streamReader.EndOfStream)
                    {
                        var cacheRow = streamReader.ReadLine();
                        if (string.IsNullOrEmpty(cacheRow))
                            continue;

                        var values = cacheRow.Split("||".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        if (values != null && values.Length == 2)
                        {
                            _cache[values[0]] = values[1];
                        }
                    }

                    cacheFile.Close();
                }
            }
        }

    }
}