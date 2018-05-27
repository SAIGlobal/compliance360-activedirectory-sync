using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compliance360.EmployeeSync.ApiV2Stream.Data;
using Newtonsoft.Json.Linq;

namespace Compliance360.EmployeeSync.ApiV2Stream.Services
{
    public interface IDivisionService
    {
        /// <summary>
        /// Sets the base uri of the Http client.
        /// </summary>
        /// <param name="baseAddress">The bsae address of the API.</param>
        void SetBaseAddress(string baseAddress);

        /// <summary>
        /// Gets a Division based on the supplied division path.
        /// </summary>
        /// <param name="divisionName">The Path of the division</param>
        /// <param name="token"></param>
        /// <returns>Entity reference to the Division</returns>
        Task<Entity> GetDivisionByNameAsync(string divisionName, string token);

        /// <summary>
        /// Gets a Division based on the supplied division path.
        /// </summary>
        /// <param name="divisionPath">The Path of the division</param>
        /// <param name="token"></param>
        /// <returns>Entity reference to the Division</returns>
        Entity GetDivisionByName(string divisionName, string token);
    }
}
