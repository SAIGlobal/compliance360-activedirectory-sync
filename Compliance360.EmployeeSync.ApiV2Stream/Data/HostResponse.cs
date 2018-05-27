using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Compliance360.EmployeeSync.ApiV2Stream.Data
{
    public class HostResponse
    {
        [JsonProperty("host")]
        public string Host { get; set; }
    }
}
