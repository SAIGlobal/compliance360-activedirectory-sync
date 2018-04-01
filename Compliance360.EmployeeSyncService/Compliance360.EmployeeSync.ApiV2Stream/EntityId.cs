using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Compliance360.EmployeeSync.ApiV2Stream
{
    public class EntityId
    {
        /// <summary>
        /// The string id token value.
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Token { get; set; }

        /// <summary>
        /// Returns the integer id of the token.
        /// </summary>
        [JsonIgnore]
        public int Id
        {
            get
            {
                var idx = Token.IndexOf(":");
                var idString = Token.Substring(idx + 1);
                int idValue;
                if (int.TryParse(idString, out idValue))
                {
                    return idValue;
                }

                return 0;
            }
        }

        /// <summary>
        /// Initializes a new EntityId
        /// </summary>
        /// <param name="token">Entity Id token.</param>
        public EntityId(string token)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            Token = token;
        }
    }
}
