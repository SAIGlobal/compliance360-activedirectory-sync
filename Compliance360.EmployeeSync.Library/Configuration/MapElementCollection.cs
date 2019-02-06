using System;
using System.Configuration;

namespace Compliance360.EmployeeSync.Library.Configuration
{
    [ConfigurationCollection(typeof(JobElement), AddItemName = "map")]
    public class MapElementCollection : ConfigurationElementCollection
    {
        public MapElement this[int idx] => BaseGet(idx) as MapElement;

        protected override ConfigurationElement CreateNewElement()
        {
            return new MapElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return Guid.NewGuid();
        }

        public MapElement Add(string from, string to, string type)
        {
            var elm = (MapElement)CreateNewElement();
            elm.From = from;
            elm.To = to;
            elm.Type = type;

            BaseAdd(elm);

            return elm;
        }
    }
}