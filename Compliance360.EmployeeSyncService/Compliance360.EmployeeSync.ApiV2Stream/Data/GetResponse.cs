using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Compliance360.EmployeeSync.ApiV2Stream.Data
{
    public class GetResponse<T>
    {
        [JsonProperty("status")]
        public ResponseStatus Status { get; set; }

        [JsonProperty("data")]
        public List<T> Data { get; set; }
    }
}
