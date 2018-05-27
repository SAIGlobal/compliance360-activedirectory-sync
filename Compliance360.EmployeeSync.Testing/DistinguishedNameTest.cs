using Compliance360.EmployeeSync.Library.Data;
using NUnit.Framework;

namespace Compliance360.EmployeeSync.Testing
{
    [TestFixture]
    public class DistinguishedNameTest
    {
        [Test]
        public void TestPopulateDistinguishedName()
        {
            var dnData = "CN=EP Web Workers,Ou=DelegateGroup,Ou=Epublish,DC=saig,DC=frd,DC=global";
            var dn = new DistinguishedName(dnData);

            Assert.AreEqual(dn.CommonName, "EP Web Workers");
            Assert.AreEqual(dn.OrganizationUnits.Count, 2);
            Assert.AreEqual(dn.OrganizationPath, "Epublish\\DelegateGroup");
            Assert.AreEqual(dn.DomainComponents.Count, 3);
            Assert.AreEqual(dn.DomainName, "saig.frd.global");
        }
    }
}