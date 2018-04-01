using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compliance360.EmployeeSync.Library.Configuration
{
    public class MapElement : ConfigurationElement
    {
        [ConfigurationProperty("from", IsRequired = true, IsKey = false)]
        public string From
        {
            get { return this["from"] as string; }
            set { this["from"] = value; }
        }

        [ConfigurationProperty("to", IsRequired = true, IsKey = false)]
        public string To
        {
            get { return this["to"] as string; }
            set { this["to"] = value; }
        }
    }
}
