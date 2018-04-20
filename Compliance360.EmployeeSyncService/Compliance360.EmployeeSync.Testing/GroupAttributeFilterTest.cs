using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compliance360.EmployeeSync.Library.Configuration;
using Compliance360.EmployeeSync.Library.Filters;
using NLog.LayoutRenderers;
using NLog.LayoutRenderers.Wrappers;
using NUnit.Framework.Internal;
using NUnit.Framework;

namespace Compliance360.EmployeeSync.Testing
{
    [TestFixture]
    public class GroupAttributeFilterTest
    {
        [Test]
        public void TestGroupFilterNoPrefix()
        {
            var result = SearchResultFactory.Construct(new 
            {
                sAMAccountName = "leetho0",
                manager = "Mike Gilbert",
                department = "development",
                givenName = "Thomas",
                sn = "Lee",
                memberOf = new string[]{
                    "CN=Administrators,Ou=Groups,Ou=Epublish,DC=saig,DC=frd,DC=global",
                    "CN=C360-Users,Ou=Groups,Ou=Epublish,DC=saig,DC=frd,DC=global"
                }
            });

            var job = new JobElement();
            job.Groups.Add("C360-Users");
            job.RemoveGroupPrefix = "";
            job.Attributes.Add("sAMAccountName");
            job.Attributes.Add("manager");
            job.Attributes.Add("department");
            job.Attributes.Add("givenName");
            job.Attributes.Add("sn");

            var memberOf = job.Attributes.Add("memberOf");
            memberOf.Filter = "GroupsAttributeFilter";

            var groupFilter = new GroupsAttributeFilter();
            var val = groupFilter.Execute(null, result, job, memberOf);

            Assert.True(val is Dictionary<string, string>);
            var groups = (Dictionary<string, string>)val;
            Assert.AreEqual("C360-Users", groups["CN=C360-Users,Ou=Groups,Ou=Epublish,DC=saig,DC=frd,DC=global"]);
        }

        [Test]
        public void TestGroupFilterWithPrefix()
        {
            var result = SearchResultFactory.Construct(new
            {
                sAMAccountName = "leetho0",
                manager = "Mike Gilbert",
                department = "development",
                givenName = "Thomas",
                sn = "Lee",
                memberOf = new string[]{
                    "CN=Administrators,Ou=Groups,Ou=Epublish,DC=saig,DC=frd,DC=global",
                    "CN=C360-Users,Ou=Groups,Ou=Epublish,DC=saig,DC=frd,DC=global"
                }
            });

            var job = new JobElement();

            // list of allowed groups
            job.Groups.Add("Users");

            // prefix to remove
            job.RemoveGroupPrefix = "C360-";

            // attributes of the result that should be mapped
            job.Attributes.Add("sAMAccountName");
            job.Attributes.Add("manager");
            job.Attributes.Add("department");
            job.Attributes.Add("givenName");
            job.Attributes.Add("sn");
            var memberOf = job.Attributes.Add("memberOf");
            
            var groupFilter = new GroupsAttributeFilter();
            var val = groupFilter.Execute(null, result, job, memberOf);

            Assert.True(val is Dictionary<string, string>);
            var groups = (Dictionary<string, string>)val;
            Assert.AreEqual("Users", groups["CN=C360-Users,Ou=Groups,Ou=Epublish,DC=saig,DC=frd,DC=global"]);
        }
    }
}
