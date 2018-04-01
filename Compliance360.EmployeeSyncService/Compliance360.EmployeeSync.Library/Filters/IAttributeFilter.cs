using System.DirectoryServices;
using Compliance360.EmployeeSync.Library.Configuration;

namespace Compliance360.EmployeeSync.Library.Filters
{
    public interface IAttributeFilter
    {
        object Execute(object currentValue,
            SearchResult result,
            JobElement jobConfig,
            AttributeElement attrib);
    }
}