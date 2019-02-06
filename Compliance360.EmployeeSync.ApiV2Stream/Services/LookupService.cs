using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using Compliance360.EmployeeSync.ApiV2Stream.Data;

namespace Compliance360.EmployeeSync.ApiV2Stream.Services
{
    public class LookupService : ILookupService
    {
        private ILogger Logger { get; }
        private IHttpDataService Http { get; }

        public LookupService(ILogger logger, IHttpDataService http)
        {
            Logger = logger;
            Http = http;
        }

        public void SetBaseAddress(string baseAddress)
        {
            Http.Initialize(baseAddress);
        }

        public Entity CreateLookupValue(string lookupFieldName, string lookupValue, string token)
        {
            return CreateLookupValueAsync(lookupFieldName, lookupValue, token).Result;
        }

        public async Task<Entity> CreateLookupValueAsync(string lookupFieldName, string lookupValue, string token)
        {
            Logger.Debug("Creating Lookup [{1}] Value [{0}]", lookupFieldName, lookupValue);

            var createLookupUri = $"/API/2.0/Data/Lookup/Employee/{lookupFieldName}?token={token}";

            var lookup = new Dictionary<string, object>
            {
                { "Text", lookupValue }
            };

            var result = await Http.PostAsync<CreateResponse>(createLookupUri, lookup);

            return new Entity { Id = result.Id };
        }

        public async Task<Entity> GetLookupValueAsync(string lookupFieldName, string lookupValue, string token)
        {
            Logger.Debug("Getting Lookup [{0}] Value [{1}]", lookupFieldName, lookupValue);

            lookupValue = lookupValue.Replace("'", "''");

            var getLookupUri = $"/API/2.0/Data/Lookup/Employee/{lookupFieldName}?select=Text&take=1&where=Text='{Uri.EscapeDataString(lookupValue)}'&token={token}";
            var resp = await Http.GetAsync<GetResponse<Entity>>(getLookupUri);

            var id = resp.Data?.FirstOrDefault()?.Id;
            return id != null ? new Entity { Id = id } : null;
        }

        public Entity GetLookupValue(string lookupFieldName, string lookupValue, string token)
        {
            return GetLookupValueAsync(lookupFieldName, lookupValue, token).Result;
        }
    }
}
