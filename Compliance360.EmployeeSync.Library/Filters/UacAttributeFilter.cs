using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compliance360.EmployeeSync.Library.Configuration;

namespace Compliance360.EmployeeSync.Library.Filters
{
    public class UacAttributeFilter : IAttributeFilter
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
            if (currentValue == null)
                return true;

            var uacFlags = (int) currentValue;
            return !Convert.ToBoolean(uacFlags & 0x0002);
        }
    }
}
