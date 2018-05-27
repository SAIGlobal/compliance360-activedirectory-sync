using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compliance360.EmployeeSync.ApiV2Stream.Data
{
    public class Folder : Entity
    {
        public string Name { get; set; }
        public Entity Parent { get; set; }
        public Entity Division { get; set; }
    }
}
