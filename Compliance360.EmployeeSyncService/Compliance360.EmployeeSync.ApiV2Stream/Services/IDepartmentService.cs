using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compliance360.EmployeeSync.ApiV2Stream.Data;
using Newtonsoft.Json.Linq;

namespace Compliance360.EmployeeSync.ApiV2Stream.Services
{
    public interface IDepartmentService
    {
        /// <summary>
        /// Sets the base uri of the Http client.
        /// </summary>
        /// <param name="baseAddress">The bsae address of the API.</param>
        void SetBaseAddress(string baseAddress);

        /// <summary>
        /// Creates a new department
        /// </summary>
        /// <param name="departmentName">The name of the department.</param>
        /// <param name="division">The division where the department will be created.</param>
        /// <param name="token">Current auth token</param>
        /// <returns>Entity reference to the department.</returns>
        Task<Entity> CreateDepartmentAsync(string departmentName, Entity division, string token);

        /// <summary>
        /// Creates a new department
        /// </summary>
        /// <param name="departmentName">The name of the department.</param>
        /// <param name="division">The division where the department will be created.</param>
        /// <param name="token">Current auth token.</param>
        /// <returns>Entity reference to the department.</returns>
        Entity CreateDepartment(string departmentName, Entity division, string token);

        /// <summary>
        /// Gets a department based on its name
        /// </summary>
        /// <param name="departmentName">The name of the department to find.</param>
        /// <param name="division">The id of the division which should contain the department.</param>
        /// <param name="token">The current active auth token.</param>
        /// <returns>Entity reference to the department.</returns>
        Task<Entity> GetDepartmentAsync(string departmentName, Entity division, string token);

        /// <summary>
        /// Gets a department based on its name
        /// </summary>
        /// <param name="departmentName">The name of the department to find.</param>
        /// <param name="division">The id of the division which should contain the department.</param>
        /// <param name="token">The current active auth token.</param>
        /// <returns>Entity reference to the department.</returns>
        Entity GetDepartment(string departmentName, Entity division, string token);
    }
}
