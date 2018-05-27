using System.Collections.Generic;
using Compliance360.EmployeeSync.Library.Configuration;
using Compliance360.EmployeeSync.Library.Data;
using Compliance360.EmployeeSync.Library.Filters;
using NUnit.Framework;

namespace Compliance360.EmployeeSync.Testing
{
    [TestFixture]
    public class UserGroupFilterTest
    {
        [Test(Description = "Should return user if filter setting is not found.")]
        public void TestFilterNoSetting()
        {
            var user = new ActiveDirectoryUser();
            var groups = new Dictionary<string, string>
            {
                {"TestGroup", "TestGroup"}
            };
            user.Attributes["memberOf"] = groups;

            var jobElement = new JobElement();
            //jobElement.FilterGroupPrefix = "";

            var filter = new UserGroupFilter();
            var filteredUser = filter.Execute(user, jobElement);
            Assert.IsNotNull(filteredUser);
        }

        [Test(Description = "Should return the user if there is at least one group.")]
        public void TestUserGroupFilterMatch()
        {
            var user = new ActiveDirectoryUser();
            var groups = new Dictionary<string, string>
            {
                {"TestGroup", "TestGroup"}
            };
            user.Attributes["memberOf"] = groups;

            var jobElement = new JobElement();
            jobElement.Groups.Add("TestGroup");

            var filter = new UserGroupFilter();
            var filteredUser = filter.Execute(user, jobElement);
            Assert.IsNotNull(filteredUser);
        }

        [Test(Description = "Should not return the user if there is not a group present")]
        public void TestUserGroupFilterNoMatch()
        {
            var user = new ActiveDirectoryUser();
            var groups = new Dictionary<string, string>();
            user.Attributes["memberOf"] = groups;

            var jobElement = new JobElement();
            jobElement.Groups.Add("NotFound");

            var filter = new UserGroupFilter();
            var filteredUser = filter.Execute(user, jobElement);
            Assert.IsNull(filteredUser);
        }
    }
}