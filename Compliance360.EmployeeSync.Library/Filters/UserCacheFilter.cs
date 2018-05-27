using System;
using System.Collections.Generic;
using System.Text;
using Compliance360.EmployeeSync.Library.Configuration;
using Compliance360.EmployeeSync.Library.Data;
using Newtonsoft.Json;
using NLog;
using System.Collections;
using System.Security.Cryptography;

namespace Compliance360.EmployeeSync.Library.Filters
{
    public class UserCacheFilter : IUserFilter, IDisposable
    {
        private bool _disposedValue; // To detect redundant calls
        private ICacheService _cacheService = null;
        private ILogger Logger { get; }
        private ICacheServiceFactory CacheServiceFactory { get; }

        /// <summary>
        ///     Initializes a new instance of the UserCacheFilter
        /// </summary>
        /// <param name="logger">The current configured logger</param>
        /// <param name="cacheServiceFactory">The factory for creating instances of the CacheService</param>
        /// <param name="job">The current job config entry.</param>
        public UserCacheFilter(ILogger logger, ICacheServiceFactory cacheServiceFactory)
        {
            Logger = logger;
            CacheServiceFactory = cacheServiceFactory;
        }

        /// <summary>
        ///     Disposes of this object, freeing resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        ///     Checks to see if the user is in the cache. If so then
        ///     validates the date. User will be filtered out if the changed
        ///     by
        /// </summary>
        /// <param name="userToFilter">Reference to the user to check.</param>
        /// <param name="jobConfig">The current job configuration.</param>
        /// <returns></returns>
        public ActiveDirectoryUser Execute(ActiveDirectoryUser userToFilter, JobElement jobConfig)
        {
            if (_cacheService == null)
            {
                _cacheService = CacheServiceFactory.CreateCacheService(Logger, jobConfig.Name);
            }
            
            var cacheKey = userToFilter.Attributes["distinguishedName"].ToString();

            var currentUserHash = BuildUserCacheHashValue(userToFilter);
            
            if (!_cacheService.ContainsKey(cacheKey))
            {
                // add the value to the cache
                _cacheService.Add(cacheKey, currentUserHash);

                // user is not in the cache...allow through
                return userToFilter;
            }

            // create a json version of the user and compare to the cached value
            // if they are not the same then the user has been updated
            var cachedUserHash = _cacheService.GetValue(cacheKey);

            if (!currentUserHash.Equals(cachedUserHash, StringComparison.InvariantCulture))
            {
                // update the cache entry
                _cacheService.Add(cacheKey, currentUserHash);
                return userToFilter;
            }

            return null;
        }

        /// <summary>
        /// Builds the cache hash value for the specified user
        /// </summary>
        /// <param name="user">The user to process</param>
        /// <returns>The hash string</returns>
        private string BuildUserCacheHashValue(ActiveDirectoryUser user)
        {
            var sb = new StringBuilder();
            foreach (var key in user.Attributes.Keys)
            {
                var val = user.Attributes[key];
                if (val is string)
                {
                    sb.Append(val);
                }
                else if (val is SortedList<string, string>)
                {
                    var list = (SortedList<string, string>) val;
                    foreach (var item in list)
                    {
                        sb.Append(item.Value);
                    }
                }
            }

            var hash = GetMd5Hash(sb.ToString());

            return hash;
        }

        /// <summary>
        /// Creates a hash based on the supplied content
        /// </summary>
        /// <param name="input">Source input to hash</param>
        /// <returns>String hash value</returns>
        static string GetMd5Hash(string input)
        {
            using (var md5Hash = MD5.Create())
            {

                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
        }

        /// <summary>
        ///     Disposes of this object, freeing resources
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _cacheService.WriteCacheEntries();
                }
                    
                _disposedValue = true;
            }
        }
    }
}