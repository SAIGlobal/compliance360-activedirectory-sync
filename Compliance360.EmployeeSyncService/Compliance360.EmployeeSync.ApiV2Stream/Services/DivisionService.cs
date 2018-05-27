using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using Compliance360.EmployeeSync.Library.Data;
using Compliance360.EmployeeSync.ApiV2Stream.Data;

namespace Compliance360.EmployeeSync.ApiV2Stream.Services
{
    public class DivisionService : IDivisionService
    {
        private ILogger Logger { get; }
        private IHttpDataService Http { get; }

        public DivisionService(ILogger logger, IHttpDataService http)
        {
            Logger = logger;
            Http = http;
        }

        public void SetBaseAddress(string baseAddress)
        {
            Http.Initialize(baseAddress);
        }
        
        public async Task<Entity> GetDivisionByNameAsync(string divisionPath, string token)
        {
            Logger.Debug("Getting division [{0}]", divisionPath);

            var findDivisionUri =
                $"/API/2.0/Data/EmployeeManagement/EmployeeDivision/Default?take=1&where=Path='{Uri.EscapeDataString(divisionPath)}'&token={token}";

            var resp = await Http.GetAsync<GetResponse<Entity>>(findDivisionUri);

            var id = resp.Data?.FirstOrDefault()?.Id;

            return id != null ? new Entity {Id = id} : null;
        }

        public Entity GetDivisionByName(string divisionPath, string token)
        {
            return GetDivisionByNameAsync(divisionPath, token).Result;
        }
    }
}
