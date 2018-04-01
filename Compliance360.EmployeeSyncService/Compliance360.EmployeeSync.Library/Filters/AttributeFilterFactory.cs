using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compliance360.EmployeeSync.Library.Data;

namespace Compliance360.EmployeeSync.Library.Filters
{
    public class AttributeFilterFactory : IAttributeFilterFactory
    {
        /// <summary>
        /// Creates an attribute filter by name
        /// </summary>
        /// <param name="filterName">Name of the attribute filter to create</param>
        /// <returns>IAttributeFilter instance</returns>
        public IAttributeFilter CreateAttributeFilter(string filterName)
        {
            var container = ContainerFactory.GetContainer();
            return container.GetInstance<IAttributeFilter>(filterName);
        }
    }
}
