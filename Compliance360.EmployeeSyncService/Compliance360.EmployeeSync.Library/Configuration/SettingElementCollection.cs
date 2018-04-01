using System.Configuration;

namespace Compliance360.EmployeeSync.Library.Configuration
{
    [ConfigurationCollection(typeof(GroupElement), AddItemName = "setting")]
    public class SettingElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new SettingElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SettingElement) element).Name;
        }

        public void Add(string name)
        {
            var elm = (SettingElement) CreateNewElement();
            elm.Name = name;
            
            BaseAdd(elm);
        }

        public new string this[string key]
        {
            get
            {
                SettingElement elm = base.BaseGet(key) as SettingElement;
                if (elm == null)
                {
                    return string.Empty;
                }

                return elm.Value;
            }
        }
    }
}