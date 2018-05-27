using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compliance360.EmployeeSync.ApiV2Stream.Data;
using Newtonsoft.Json.Linq;

namespace Compliance360.EmployeeSync.ApiV2Stream.Services
{
    public interface IAuthenticationService
    {
        /// <summary>
        /// Get the API host address for the specified organization.
        /// </summary>
        /// <param name="organization">Organization name</param>
        /// <returns>Address of the API for the organization.</returns>
        Task<string> GetHostAddressAsync(string baseAddress, string organization);

        /// <summary>
        /// Get the API host address for the specified organization.
        /// </summary>
        /// <param name="organization">Organization name</param>
        /// <returns>Address of the API for the organization.</returns>
        string GetHostAddress(string baseAddress, string organization);

        /// <summary>
        /// Logs into the C360 API. Returns the API auth token.
        /// </summary>
        /// <param name="baseAddress">The base uri of the api.</param>
        /// <param name="organization">The organization name.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>Authentication token</returns>
        Task<string> LoginAsync(string baseAddress, string organization, string username, string password);

        /// <summary>
        /// Logs into the C360 API. Returns the API auth token.
        /// </summary>
        /// <param name="baseAddress">The base uri of the api.</param>
        /// <param name="organization">The organization name.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>Authentication token</returns>
        string Login(string baseAddress, string organization, string username, string password);

        /// <summary>
        /// Logs the user out of the C360 application terminating the session.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>True, if successful</returns>
        Task<bool> LogoutAsync(string token);

        /// <summary>
        /// Logs the user out of the C360 application terminating the session.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>True, if successful</returns>
        void Logout(string token);
    }
}
