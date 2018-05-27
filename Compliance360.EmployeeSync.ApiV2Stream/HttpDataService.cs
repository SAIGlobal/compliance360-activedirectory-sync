using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Compliance360.EmployeeSync.Library.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Compliance360.EmployeeSync.ApiV2Stream
{
    public sealed class HttpDataService : IHttpDataService
    {
        public IHttpClientHandler Client { get; set; }
        private ILogger Logger { get; }

        public HttpDataService(ILogger logger, IHttpClientHandler client)
        {
            Logger = logger;
            Client = client;
        }

        public void Initialize(string baseAddress)
        {
            Client.Initialize(baseAddress);
        }

        public async Task<T> GetAsync<T>(string uri)
        {
            var resp = await Client.GetAsync(uri);
            if (!resp.IsSuccessStatusCode)
            {
                throw new DataException($"Error getting data at endpoint:{uri}\n({resp.StatusCode}): {resp.ReasonPhrase}");
            }

            var respContent = await resp.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<T>(respContent);

            return result;
        }

        public async Task GetAsync(string uri)
        {
            var resp = await Client.GetAsync(uri);
            if (!resp.IsSuccessStatusCode)
            {
                throw new DataException($"Error getting data at endpoint:{uri}\n({resp.StatusCode}): {resp.ReasonPhrase}");
            }
        }

        public async Task<T> PostAsync<T>(string uri, object data)
        {
            var jsonData = JsonConvert.SerializeObject(data);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var resp = await Client.PostAsync(uri, content);
            if (!resp.IsSuccessStatusCode)
            {
                throw new DataException($"Error posting to endpoint:{uri}\n({resp.StatusCode}): {resp.ReasonPhrase}\n{jsonData}");
            }
            
            var respContent = await resp.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<T>(respContent);

            return result;
        }
    }
}
