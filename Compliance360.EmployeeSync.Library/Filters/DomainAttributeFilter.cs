using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compliance360.EmployeeSync.Library.Configuration;
using Compliance360.EmployeeSync.Library.Data;

namespace Compliance360.EmployeeSync.Library.Filters
{
    public class DomainAttributeFilter : IAttributeFilter
    {
        /// <summary>
        ///     This filter reads a value from the search result
        /// </summary>
        /// <param name="currentValue">The current result value</param>
        /// <param name="result">The ldap result</param>
        /// <param name="jobElement">The configuration job element</param>
        /// <param name="attrib">The current attribute element</param>
        /// <returns></returns>
        public object Execute(object currentValue, SearchResult result, JobElement jobElement, AttributeElement attrib)
        {
            const string propName = "distinguishedName";

            if (!result.Properties.Contains(propName))
                return null;

            var propValues = result.Properties[propName];
            if (propValues != null && propValues.Count > 0)
            {
                var dn = new DistinguishedName(propValues[0].ToString());
                return dn.DomainComponents.Count > 0 ? dn.DomainComponents[0] : null;
            }
                
            return null;
        }
    }
}
