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
    public class DepartmentService : IDepartmentService
    {
        private ILogger Logger { get; }
        private IHttpDataService Http { get; }

        public DepartmentService(ILogger logger, IHttpDataService http)
        {
            Logger = logger;
            Http = http;
        }

        public void SetBaseAddress(string baseAddress)
        {
            Http.Initialize(baseAddress);
        }

        public async Task<Entity> CreateDepartmentAsync(string departmentName, Entity division, string token)
        {
            Logger.Debug("Creating department [{0}]", departmentName);

            var createDepartmentUri = $"/API/2.0/Data/EmployeeManagement/EmployeeDepartment/Default?token={token}";

            var department = new Dictionary<string, object>
            {
                { "DeptNum", departmentName },
                { "DeptName", departmentName },
                { "Division", division }
            };

            var result = await Http.PostAsync<CreateResponse>(createDepartmentUri, department);

            return new Entity {Id = result.Id};
        }

        public Entity CreateDepartment(string departmentName, Entity division, string token)
        {
            return CreateDepartmentAsync(departmentName, division, token).Result;
        }
        
        public async Task<Entity> GetDepartmentAsync(
            string departmentName,
            Entity division,
            string token)
        {
            Logger.Debug("Getting department [{0}]", departmentName);

            departmentName = departmentName.Replace("'", "''");

            var where =
                $"((DeptNum='{Uri.EscapeDataString(departmentName)}')|(DeptName='{Uri.EscapeDataString(departmentName)}'))";

            var findDepartmentUri =
                $"/API/2.0/Data/EmployeeManagement/EmployeeDepartment/Default?take=1&where={where}&token={token}";

            var resp = await Http.GetAsync<GetResponse<Entity>>(findDepartmentUri);

            return new Entity {Id = resp.Data?.FirstOrDefault()?.Id};
        }

        public Entity GetDepartment(
            string departmentName,
            Entity division,
            string token)
        {
            return GetDepartmentAsync(departmentName, division, token).Result;
        }
    }
}
