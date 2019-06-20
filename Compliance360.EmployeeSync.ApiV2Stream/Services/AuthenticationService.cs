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
    public class AuthenticationService : IAuthenticationService
    {
        private ILogger Logger { get; }
        private IHttpDataService Http { get; }

        public AuthenticationService(ILogger logger, IHttpDataService http)
        {
            Logger = logger;
            Http = http;
        }

        public async Task<string> GetHostAddressAsync(string baseAddress, string organization)
        {
            Logger.Debug("Getting Host Address for organization [{0}]", organization);

            var orgHostUri = $"/API/2.0/Security/OrganizationHost?organization={Uri.EscapeDataString(organization)}";
            Http.Initialize(baseAddress);
            var resp = await Http.GetAsync<HostResponse>(orgHostUri);

            if (resp.Host == null)
            {
                throw new DataException($"Cannot get organization host address at: {orgHostUri}");
            }

            return resp.Host;
        }

        public string GetHostAddress(string baseAddress, string organization)
        {
            return GetHostAddressAsync(baseAddress, organization).Result;
        }
        
        public async Task<string> AuthenticateAsync(string baseAddress, string organization, string integrationKey, string culture)
        {
            Logger.Debug("Logging in to API Organization:{0} IntegrationKey:{1}", organization, integrationKey);

            Http.Initialize(baseAddress);

            var loginData = new
            {
                organization,
                integrationKey,
                culture
            };

            const string loginUri = "/API/2.0/Security/Authenticate";

            var resp = await Http.PostAsync<LoginResponse>(loginUri, loginData);

            return resp.Token;
        }

        public string Authenticate(string baseAddress, string organization, string integrationKey, string culture)
        {
            return AuthenticateAsync(baseAddress, organization, integrationKey, culture).Result;
        }


        public async Task<bool> LogoutAsync(string token)
        {
            Logger.Debug("Logging out of API");

            var logoutUri = $"/API/2.0/Security/Logout?token={Uri.EscapeUriString(token)}";
            await Http.GetAsync(logoutUri);

            return true;
        }

        public void Logout(string token)
        {
            LogoutAsync(token).GetAwaiter().GetResult();
        }
    }
}
