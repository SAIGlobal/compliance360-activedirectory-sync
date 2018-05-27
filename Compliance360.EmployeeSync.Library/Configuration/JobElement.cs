using System.Configuration;

namespace Compliance360.EmployeeSync.Library.Configuration
{
    public class JobElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return this["name"] as string; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("domain", IsRequired = true)]
        public string Domain
        {
            get { return this["domain"] as string; }
            set { this["domain"] = value; }
        }

        [ConfigurationProperty("ou")]
        public string Ou
        {
            get { return this["ou"] as string; }
            set { this["ou"] = value; }
        }

        [ConfigurationProperty("ldapQuery")]
        public string LdapQuery
        {
            get { return this["ldapQuery"] as string; }
            set { this["ldapQuery"] = value; }
        }

        [ConfigurationProperty("intervalSeconds")]
        public int IntervalSeconds
        {
            get
            {
                var val = this["intervalSeconds"];
                return (int) val;
            }
            set { this["intervalSeconds"] = value; }
        }


        [ConfigurationProperty("removeGroupPrefix")]
        public string RemoveGroupPrefix
        {
            get { return this["removeGroupPrefix"] as string; }
            set { this["removeGroupPrefix"] = value; }
        }

        
        [ConfigurationProperty("allowedGroups")]
        public GroupElementCollection Groups
        {
            get
            {
                var val = (GroupElementCollection)this["allowedGroups"];
                if (val == null)
                {
                    val = new GroupElementCollection();
                    this["allowedGroups"] = val;
                }
                return val;
            }
        } 

        [ConfigurationProperty("attributes")]
        public AttributeElementCollection Attributes
        {
            get
            {
                var val = (AttributeElementCollection) this["attributes"];
                if (val == null)
                {
                    val = new AttributeElementCollection();
                    this["attributes"] = val;
                }
                return val;
            }
        }

        [ConfigurationProperty("type")]
        public string Type
        {
            get { return this["type"] as string; }
            set { this["type"] = value; }
        }

        [ConfigurationProperty("userName")]
        public string Username
        {
            get { return this["userName"] as string; }
            set { this["userName"] = value; }
        }

        [ConfigurationProperty("password")]
        public string Password
        {
            get { return this["password"] as string; }
            set { this["password"] = value; }
        }

        [ConfigurationProperty("outputStreams")]
        public StreamElementCollection OutputStreams
        {
            get
            {
                var val = (StreamElementCollection)this["outputStreams"];
                if (val == null)
                {
                    val = new StreamElementCollection();
                    this["outputStreams"] = val;
                }
                return val;
            }
        }

        public override bool IsReadOnly()
        {
            return false;
        }
    }
}