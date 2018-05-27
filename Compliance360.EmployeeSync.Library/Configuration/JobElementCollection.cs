using System.Configuration;

namespace Compliance360.EmployeeSync.Library.Configuration
{
    [ConfigurationCollection(typeof(JobElement), AddItemName = "job")]
    public class JobElementCollection : ConfigurationElementCollection
    {
        public JobElement this[int idx] => BaseGet(idx) as JobElement;

        protected override ConfigurationElement CreateNewElement()
        {
            return new JobElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((JobElement) element).Name;
        }

        public void Add(string jobName)
        {
            var elm = (JobElement)CreateNewElement();
            elm.Name = jobName;

            BaseAdd(elm);
        }
    }
}