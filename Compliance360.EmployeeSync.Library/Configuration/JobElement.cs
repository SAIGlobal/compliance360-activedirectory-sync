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

        [ConfigurationProperty("errorThreshold")]
        public int ErrorThreshold
        {
            get
            {
                var val = this["errorThreshold"];
                return (int)val;
            }
            set { this["errorThreshold"] = value; }
        }

        [ConfigurationProperty("errorNotificationHost")]
        public string ErrorNotificationHost
        {
            get { return this["errorNotificationHost"] as string; }
            set { this["errorNotificationHost"] = value; }
        }

        [ConfigurationProperty("errorNotificationPort")]
        public int ErrorNotificationPort
        {
            get
            {
                var val = this["errorNotificationPort"];
                return (int)val;
            }
            set { this["errorNotificationPort"] = value; }
        }

        [ConfigurationProperty("errorNotificationUserName")]
        public string ErrorNotificationUserName
        {
            get { return this["errorNotificationUserName"] as string; }
            set { this["errorNotificationUserName"] = value; }
        }

        [ConfigurationProperty("errorNotificationPassword")]
        public string ErrorNotificationPassword
        {
            get { return this["errorNotificationPassword"] as string; }
            set { this["errorNotificationPassword"] = value; }
        }

        [ConfigurationProperty("errorNotificationDomain")]
        public string ErrorNotificationDomain
        {
            get { return this["errorNotificationDomain"] as string; }
            set { this["errorNotificationDomain"] = value; }
        }

        [ConfigurationProperty("errorNotificationEmailFrom")]
        public string ErrorNotificationEmailFrom
        {
            get { return this["errorNotificationEmailFrom"] as string; }
            set { this["errorNotificationEmailFrom"] = value; }
        }

        [ConfigurationProperty("errorNotificationEmailTo")]
        public string ErrorNotificationEmailTo
        {
            get { return this["errorNotificationEmailTo"] as string; }
            set { this["errorNotificationEmailTo"] = value; }
        }

        [ConfigurationProperty("errorNotificationSubject")]
        public string ErrorNotificationSubject
        {
            get { return this["errorNotificationSubject"] as string; }
            set { this["errorNotificationSubject"] = value; }
        }

        [ConfigurationProperty("removeGroupPrefix")]
        public string RemoveGroupPrefix
        {
            get { return this["removeGroupPrefix"] as string; }
            set { this["removeGroupPrefix"] = value; }
        }

        [ConfigurationProperty("errorNotificationUseSsl")]
        public bool ErrorNotificationUseSsl
        {
            get { return (bool)this["errorNotificationUseSsl"]; }
            set { this["errorNotificationUseSsl"] = value; }
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