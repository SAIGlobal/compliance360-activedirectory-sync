using System;
using Newtonsoft.Json;

namespace Compliance360.EmployeeSync.ApiV2Stream.Data
{
    public class Entity
    {
        /// <summary>
        /// The string id token value.
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        
        /// <summary>
        /// Returns the InstanceId of the entity
        /// </summary>
        [JsonIgnore]
        public int InstanceId
        {
            get
            {
                var idx = Id.IndexOf(":");
                var idString = Id.Substring(idx + 1);
                int idValue;
                if (int.TryParse(idString, out idValue))
                {
                    return idValue;
                }

                return 0;
            }
        }
    }
}
