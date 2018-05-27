using System.Configuration;

namespace Compliance360.EmployeeSync.Library.Configuration
{
    [ConfigurationCollection(typeof(GroupElement), AddItemName = "group")]
    public class GroupElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new GroupElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((GroupElement) element).Name;
        }

        public void Add(string groupName)
        {
            var elm = (GroupElement) CreateNewElement();
            elm.Name = groupName;

            BaseAdd(elm);
        }
    }
}