using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Compliance360.EmployeeSync.Library.Configuration;

namespace Compliance360.EmployeeSync.Library.Filters
{
    public class SidToStringAttributeFilter: IAttributeFilter
    {
        public object Execute(object currentValue, SearchResult result, JobElement config, AttributeElement attrib)
        {
            try
            {
                var sid = new SecurityIdentifier((byte[]) currentValue, 0);
                return sid.ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
