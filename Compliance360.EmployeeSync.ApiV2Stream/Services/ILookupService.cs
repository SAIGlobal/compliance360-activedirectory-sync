using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compliance360.EmployeeSync.ApiV2Stream.Data;
using Newtonsoft.Json.Linq;

namespace Compliance360.EmployeeSync.ApiV2Stream.Services
{
    public interface ILookupService
    {
        /// <summary>
        /// Sets the base HTTP address for the service.
        /// </summary>
        /// <param name="baseAddress">The base http address.</param>
        void SetBaseAddress(string baseAddress);
        
        /// <summary>
        /// Creates a new Lookup entry.
        /// </summary>
        /// <param name="lookupFieldName">The name of the lookup field.</param>
        /// <param name="lookupValue">The value to find.</param>
        /// <param name="token">The active AuthToken.</param>
        /// <returns>The new lookup Entity</returns>
        Task<Entity> CreateLookupValueAsync(string lookupFieldName, string lookupValue, string token);

        /// <summary>
        /// Creates a new Lookup entry.
        /// </summary>
        /// <param name="lookupFieldName">The name of the lookup field.</param>
        /// <param name="lookupValue">The value to find.</param>
        /// <param name="token">The active AuthToken.</param>
        /// <returns>The new lookup Entity</returns>
        Entity CreateLookupValue(string lookupFieldName, string lookupValue, string token);

        /// <summary>
        /// Gets a lookup value based on its field name and value.
        /// </summary>
        /// <param name="lookupFieldName">The name of the lookup field.</param>
        /// <param name="lookupValue">The value to find.</param>
        /// <param name="token">The active AuthToken.</param>
        /// <returns>The Lookup Entity if it was found or null</returns>
        Task<Entity> GetLookupValueAsync(string lookupFieldName, string lookupValue, string token);

        /// <summary>
        /// Gets a lookup value based on its field name and value.
        /// </summary>
        /// <param name="lookupFieldName">The name of the lookup field.</param>
        /// <param name="lookupValue">The value to find.</param>
        /// <param name="token">The active AuthToken.</param>
        /// <returns>The Lookup Entity if it was found or null</returns>
        Entity GetLookupValue(string lookupFieldName, string lookupValue, string token);
    }
}
