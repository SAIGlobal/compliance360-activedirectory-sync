using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compliance360.EmployeeSync.ApiV2Stream
{
    public interface IHttpDataService
    {
        /// <summary>
        /// Initializes the HttpClient to use the base uri
        /// </summary>
        /// <param name="baseAddress">The base uri for all requests.</param>
        void Initialize(string baseAddress);

        /// <summary>
        /// Performs a GET operation for specified uri, returning the specified type.
        /// </summary>
        /// <typeparam name="T">Type of data to return.</typeparam>
        /// <param name="uri">The Uri of the GET operation.</param>
        /// <returns>The object data of type T from the GET operation.</returns>
        Task<T> GetAsync<T>(string uri);

        /// <summary>
        /// Performs a POST operation for specified uri, returning the specified type.
        /// </summary>
        /// <typeparam name="T">Type of data to return.</typeparam>
        /// <param name="uri">The Uri of the POST operation.</param>
        /// <param name="data">The data to send to the POST uri.</param>
        /// <returns>The object data of type T from the POST operation.</returns>
        Task<T> PostAsync<T>(string uri, object data);

        /// <summary>
        /// Performs a GET operation for specified uri.
        /// </summary>
        /// <param name="uri">The Uri of the GET operation.</param>
        Task GetAsync(string uri);
    }
}
