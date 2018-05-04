using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compliance360.EmployeeSync.ApiV2Stream.Data
{
    public class Profile : Entity
    {
        public List<EntityReference> Groups { get; set; }
    }
}
