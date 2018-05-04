using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Compliance360.EmployeeSync.ApiV2Stream
{
    public class Employee : Dictionary<string, object>
    {
        public string Id
        {
            get { return this.ContainsKey("id") ? this["id"] as string  : null; }
            set { this["id"] = value; }
        }

        /// <summary>
        /// Returns the InstanceId of the employee
        /// </summary>
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
