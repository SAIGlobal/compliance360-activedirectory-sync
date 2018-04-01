using System.Configuration;

namespace Compliance360.EmployeeSync.Library.Configuration
{
    public class StreamElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return this["name"] as string; }
            set { this["name"] = value; }
        }
        
        [ConfigurationProperty("settings")]
        public SettingElementCollection Settings
        {
            get
            {
                var val = (SettingElementCollection)this["settings"];
                if (val == null)
                {
                    val = new SettingElementCollection();
                    this["settings"] = val;
                }
                return val;
            }
        }

        [ConfigurationProperty("mapping")]
        public MapElementCollection Mapping
        {
            get
            {
                var val = (MapElementCollection)this["mapping"];
                if (val == null)
                {
                    val = new MapElementCollection();
                    this["mapping"] = val;
                }
                return val;
            }
        }
    }
}