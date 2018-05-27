using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compliance360.EmployeeSync.ApiV2Stream.Data
{
    public class Relationship : Entity
    {
        public Entity Employee { get; set; }
        public Entity Type { get; set; }
    }
}
