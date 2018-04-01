using System.DirectoryServices;
using Compliance360.EmployeeSync.Library.Configuration;

namespace Compliance360.EmployeeSync.Library.Filters
{
    public class ReadLdapFilter : IAttributeFilter
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
            var propValues = result.Properties[attrib.Name];
            if (propValues != null && propValues.Count > 0)
                return propValues[0];

            return null;
        }
    }
}