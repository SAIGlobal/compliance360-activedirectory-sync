using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compliance360.EmployeeSync.ApiV2Stream.Data;

namespace Compliance360.EmployeeSync.ApiV2Stream.Services
{
    public interface ICompanyService
    {
        /// <summary>
        /// Sets the base HTTP address for the service.
        /// </summary>
        /// <param name="baseAddress">The base http address.</param>
        void SetBaseAddress(string baseAddress);

        /// <summary>
        /// Creates a new Company entity.
        /// </summary>
        /// <param name="companyName">The name of the company.</param>
        /// <param name="token">The active AuthToken.</param>
        /// <returns>The new Company Entity</returns>
        Task<Entity> CreateCompanyAsync(string companyName, string token);

        /// <summary>
        /// Creates a new Company entity.
        /// </summary>
        /// <param name="companyName">The name of the company.</param>
        /// <param name="token">The active AuthToken.</param>
        /// <returns>The new Company Entity</returns>
        Entity CreateCompany(string companyName, string token);

        /// <summary>
        /// Gets a Company by its name.
        /// </summary>
        /// <param name="companyName">The name of the company.</param>
        /// <param name="token">The active AuthToken.</param>
        /// <returns>The found Company or null</returns>
        Task<Entity> GetCompanyAsync(string companyName, string token);

        /// <summary>
        /// Gets a Company by its name.
        /// </summary>
        /// <param name="companyName">The name of the company.</param>
        /// <param name="token">The active AuthToken.</param>
        /// <returns>The found Company or null</returns>
        Entity GetCompany(string companyName, string token);
    }
}
