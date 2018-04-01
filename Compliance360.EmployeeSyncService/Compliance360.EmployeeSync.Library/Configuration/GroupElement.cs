using System.Configuration;

namespace Compliance360.EmployeeSync.Library.Configuration
{
    public class GroupElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return this["name"] as string; }
            set { this["name"] = value; }
        }
    }
}