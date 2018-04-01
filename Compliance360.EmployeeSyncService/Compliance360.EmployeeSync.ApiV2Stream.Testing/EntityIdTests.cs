using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Compliance360.EmployeeSync.ApiV2Stream.Testing
{
    [TestFixture]
    public class EntityIdTests
    {
        [Test]
        public void CreateNewEntityId()
        {
            var token = "EmployeeManagement/EmployeeDivision/Default:89";
            var entityId = new EntityId(token);

            Assert.AreEqual(token, entityId.Token);
            Assert.AreEqual(89, entityId.Id);
        }
    }
}
