using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compliance360.EmployeeSync.Library.Configuration;

namespace Compliance360.EmployeeSync.Library.Filters
{
    public class GuidToStringAttributeFilter : IAttributeFilter
    {
        public object Execute(object currentValue, SearchResult result, JobElement jobConfig, AttributeElement attrib)
        {
            try
            {
                return new Guid((System.Byte[]) currentValue).ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
