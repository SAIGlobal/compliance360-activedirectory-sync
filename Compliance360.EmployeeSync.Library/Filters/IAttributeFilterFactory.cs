using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compliance360.EmployeeSync.Library.Filters
{
    public interface IAttributeFilterFactory
    {
        IAttributeFilter CreateAttributeFilter(string filterName);
    }
}
