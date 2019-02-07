using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using Compliance360.EmployeeSync.ApiV2Stream.Data;

namespace Compliance360.EmployeeSync.ApiV2Stream.Services
{
    public class CompanyService : ICompanyService
    {
        private ILogger Logger { get; }
        private IHttpDataService Http { get; }

        public CompanyService(ILogger logger, IHttpDataService http)
        {
            Logger = logger;
            Http = http;
        }

        public void SetBaseAddress(string baseAddress)
        {
            Http.Initialize(baseAddress);
        }

        public Entity CreateCompany(string companyName, string token)
        {
            return CreateCompanyAsync(companyName, token).Result;
        }

        public async Task<Entity> CreateCompanyAsync(string companyName, string token)
        {
            Logger.Debug("Creating Company [{0}]", companyName);

            var createCompanypUri = $"/API/2.0/Data/EmployeeManagement/EmployeeCompany/Default?token={token}";

            var company = new Dictionary<string, object>
            {
                { "CompName", companyName }
            };

            var result = await Http.PostAsync<CreateResponse>(createCompanypUri, company);

            return new Entity { Id = result.Id };
        }

        public Entity GetCompany(string companyName, string token)
        {
            return GetCompanyAsync(companyName, token).Result;
        }

        public async Task<Entity> GetCompanyAsync(string companyName, string token)
        {
            Logger.Debug("Getting Company [{0}]", companyName);

            companyName = companyName.Replace("'", "''");

            var getCompanyUri = $"/API/2.0/Data/EmployeeManagement/EmployeeCompany/Default?select=CompName&take=1&where=CompName='{Uri.EscapeDataString(companyName)}'&token={token}";
            var resp = await Http.GetAsync<GetResponse<Entity>>(getCompanyUri);

            var id = resp.Data?.FirstOrDefault()?.Id;
            return id != null ? new Entity { Id = id } : null;
        }
    }
}
