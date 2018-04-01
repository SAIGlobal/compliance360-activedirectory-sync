using Compliance360.EmployeeSync.Library.Configuration;
using Compliance360.EmployeeSync.Library.Data;

namespace Compliance360.EmployeeSync.Library.Filters
{
    public interface IUserFilter
    {
        ActiveDirectoryUser Execute(ActiveDirectoryUser userToFilter, JobElement jobConfig);
    }
}