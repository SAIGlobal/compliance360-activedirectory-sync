using System.Configuration;

namespace Compliance360.EmployeeSync.Library.Configuration
{
    [ConfigurationCollection(typeof(AttributeElement), AddItemName = "stream")]
    public class StreamElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new StreamElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((StreamElement) element).Name;
        }

        public StreamElement Add(string streamName)
        {
            var elm = (StreamElement)CreateNewElement();
            elm.Name = streamName;

            BaseAdd(elm);

            return elm;
        }

        public new StreamElement this[string name]
        {
            get
            {
                var elm = BaseGet(name);
                return elm as StreamElement;
            }
        }
    }
}