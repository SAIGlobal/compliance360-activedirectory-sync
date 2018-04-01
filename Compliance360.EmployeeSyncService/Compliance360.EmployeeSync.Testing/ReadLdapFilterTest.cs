using Compliance360.EmployeeSync.Library.Configuration;
using Compliance360.EmployeeSync.Library.Data;
using Compliance360.EmployeeSync.Library.Filters;
using NUnit.Framework;

namespace Compliance360.EmployeeSync.Testing
{
    [TestFixture]
    public class ReadLdapFilterTest
    {
        [Test]
        public void TestReadLdapMissingAttribute()
        {
            var filter = new ReadLdapFilter();
            var attrib = new AttributeElement
            {
                Name = "NotFound"
            };
        
            // mock the ldap search result
            var result = SearchResultFactory.Construct(new
            {
                givenName = "Thomas"
            });

            var value = filter.Execute(null, result, null, attrib);
            Assert.IsNull(value);
        }

        [Test]
        public void TestReadLdapValue()
        {
            var filter = new ReadLdapFilter();
            var attrib = new AttributeElement
            {
                Name = "givenName"
            };

            // mock the ldap search result
            var result = SearchResultFactory.Construct(new
            {
                givenName = "Thomas"
            });


            var value = filter.Execute(null, result, null, attrib);
            Assert.AreEqual(value, "Thomas");
        }
    }
}