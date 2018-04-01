using System;
using System.Collections.Generic;
using Compliance360.EmployeeSync.Library.Configuration;

namespace Compliance360.EmployeeSync.Library.Data
{
    public interface IActiveDirectoryService
    {
        IEnumerable<ActiveDirectoryUser> GetActiveDirectoryUsers(JobElement jobConfig);
    }
}