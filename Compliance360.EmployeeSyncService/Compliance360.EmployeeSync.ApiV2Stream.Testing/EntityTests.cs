using Compliance360.EmployeeSync.ApiV2Stream.Data;
using NUnit.Framework;

namespace Compliance360.EmployeeSync.ApiV2Stream.Testing
{
    [TestFixture]
    public class EntityTests
    {
        [Test]
        public void CreateNewEntityId()
        {
            var token = "EmployeeManagement/EmployeeDivision/Default:89";
            var entity = new Entity { Id = token };

            Assert.AreEqual(token, entity.Id);
            Assert.AreEqual(89, entity.InstanceId);
        }
    }
}
