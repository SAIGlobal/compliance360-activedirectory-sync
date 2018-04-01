using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compliance360.EmployeeSync.Library.Filters
{
    public interface IUserFilterFactory
    {
        IUserFilter CreateUserFilter(string filterName);
    }
}
