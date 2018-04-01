using System.Configuration;

namespace Compliance360.EmployeeSync.Library.Configuration
{
    [ConfigurationCollection(typeof(AttributeElement), AddItemName = "attribute")]
    public class AttributeElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new AttributeElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((AttributeElement) element).Name;
        }

        public AttributeElement Add(string attributeName)
        {
            var elm = (AttributeElement)CreateNewElement();
            elm.Name = attributeName;

            BaseAdd(elm);

            return elm;
        }
    }
}