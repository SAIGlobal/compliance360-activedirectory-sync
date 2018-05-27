using System.Configuration;

namespace Compliance360.EmployeeSync.Library.Configuration
{
    public class AttributeElement : ConfigurationElement
    {
        [ConfigurationProperty("alias", IsRequired = false, IsKey = false)]
        public string Alias
        {
            get { return this["alias"] as string; }
            set { this["alias"] = value; }
        }

        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return this["name"] as string; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("filter", IsRequired = false, IsKey = false)]
        public string Filter
        {
            get { return this["filter"] as string; }
            set { this["filter"] = value; }
        }

        [ConfigurationProperty("includeInQuery", IsRequired = false, IsKey = false)]
        public bool IncludeInQuery
        {
            get
            {
                var val = this["includeInQuery"] as string;
                if (!string.IsNullOrEmpty(val))
                {
                    bool include;
                    if (bool.TryParse(val, out include))
                        return include;
                }

                return true;
            }
            set { this["includeInQuery"] = value.ToString(); }
        }
    }
}