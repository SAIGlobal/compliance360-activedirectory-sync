using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compliance360.EmployeeSync.Library.Filters
{
    public class UserFilterFactory : IUserFilterFactory
    {
        public IUserFilter CreateUserFilter(string filterName)
        {
            var container = ContainerFactory.GetContainer();
            return container.GetInstance<IUserFilter>(filterName);
        }
    }
}
