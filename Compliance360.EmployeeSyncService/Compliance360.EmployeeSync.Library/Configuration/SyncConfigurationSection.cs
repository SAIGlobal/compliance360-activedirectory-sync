using System.Configuration;

namespace Compliance360.EmployeeSync.Library.Configuration
{
    public class SyncConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("jobs")]
        public JobElementCollection Jobs
        {
            get
            {
                var val = (JobElementCollection) this["jobs"];
                if (val == null)
                {
                    val = new JobElementCollection();
                    this["jobs"] = val;
                }
                return val;
            }
        }
    }
}