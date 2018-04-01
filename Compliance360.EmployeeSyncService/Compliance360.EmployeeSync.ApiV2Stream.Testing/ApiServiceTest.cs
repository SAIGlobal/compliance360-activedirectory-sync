using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compliance360.EmployeeSync.ApiV2Stream;
using Compliance360.EmployeeSync.Library;
using Compliance360.EmployeeSync.Library.Configuration;
using Compliance360.EmployeeSync.Library.Data;
using NLog;
using NUnit.Framework;

namespace Compliance360.EmployeeSync.ApiV2Stream.Testing
{
    [TestFixture(Description = "Integration tests for API service calls. " +
                               "These test require a live API in order to work correctly.")]
    [Explicit]
    public class ApiV2ServiceTest
    {
        private ApiService _apiSvc;
        private string _token;

        [OneTimeSetUp]
        public void BeforeAll()
        {
            var logger = ContainerFactory.GetContainer().GetInstance<ILogger>();
            var config = ContainerFactory.GetContainer().GetInstance<IConfigurationService>().GetConfig();
            _apiSvc = new ApiService(logger);

            var jobConfig = config.Jobs[0];
            var streamConfig = jobConfig.OutputStreams["Compliance360ApiV2"];

            _token = _apiSvc.LoginAsync(streamConfig.Settings["baseAddress"],
                streamConfig.Settings["organization"],
                streamConfig.Settings["username"],
                streamConfig.Settings["password"]).Result;
        }

        [OneTimeTearDown]
        public void AfterAll()
        {
            _apiSvc.LogoutAsync(_token).Wait();
        }

        [Test(Description = "Tests the ability to create a department")]
        public void TestCreateDepartment()
        {
            var division = new EntityId("EmployeeManagement/EmployeeDivision/Default:1");
            var department = _apiSvc.CreateDepartmentAsync("TEST_DEPARTMENT", division, _token).Result;
            Assert.IsNotNull(department);
        }

        [Test(Description = "Tests the ability to find a department")]
        public void TestFindDepartment()
        {
            var division = new EntityId("EmployeeManagement/EmployeeDivision/Default:1");
            var department = _apiSvc.GetDepartmentAsync("TEST_DEPARTMENT", division, _token).Result;
            Assert.IsNotNull(department);
        }
        

        [Test(Description = "Tests ability to login using app.config stream settings.")]
        public void TestLoginSuccess()
        {
            Assert.IsNotNull(_token);
        }

        [Test(Description = "Test ability to logout")]
        public void TestLogout()
        {
            var logger = ContainerFactory.GetContainer().GetInstance<ILogger>();
            var config = ContainerFactory.GetContainer().GetInstance<IConfigurationService>().GetConfig();

            var jobConfig = config.Jobs[0];
            var streamConfig = jobConfig.OutputStreams["Compliance360ApiV2"];

            var apiSvc = new ApiService(logger);
            var token = apiSvc.LoginAsync(streamConfig.Settings["baseAddress"],
                streamConfig.Settings["organization"],
                streamConfig.Settings["username"],
                streamConfig.Settings["password"]).Result;
            Assert.IsNotNull(token);

            apiSvc.LogoutAsync(token).Wait();
        }

        [Test(Description = "Tests ability to createa new job title.")]
        public void TestCreateJobTitle()
        {
            var division = new EntityId("EmployeeManagement/EmployeeDivision/Default:1");
            var jobTitle = _apiSvc.CreateJobTitleAsync("TEST_TITLE", division, _token).Result;

            Assert.IsNotNull(jobTitle);
        }

        [Test(Description = "Tests ability to return job titles from the api")]
        public void TestGetJobTitle()
        {
            var division = new EntityId("EmployeeManagement/EmployeeDivision/Default:1");
            var jobTitle = _apiSvc.GetJobTitleAsync("TEST_TITLE", division, _token).Result;

            Assert.IsNotNull(jobTitle);
        }

        [Test(Description = "Tests ability to handle missing job title from the api")]
        public void TestGetMissingJobTitle()
        {
            var division = new EntityId("EmployeeManagement/EmployeeDivision/Default:1");
            var jobTitle = _apiSvc.GetJobTitleAsync("NOT_FOUND", division, _token).Result;

            Assert.IsNull(jobTitle);
        }

        [Test]
        public void TestGetDivision()
        {
            var divisionId = _apiSvc.GetDivisionAsync("Main Division", _token).Result;

            Assert.IsNotNull(divisionId);
        }

        [Test]
        public void TestGetMissingDivision()
        {
            var divisionId = _apiSvc.GetDivisionAsync("NOT_FOUND", _token).Result;

            Assert.IsNull(divisionId);
        }

        [Test(Description = "Tests ability to create a new group.")]
        public void TestCreateGroup()
        {
            var division = new EntityId("EmployeeManagement/EmployeeDivision/Default:1");
            var group = _apiSvc.CreateGroupAsync("TEST_GROUP", _token).Result;

            Assert.IsNotNull(group);
        }

        [Test(Description = "Tests ability to find an existing group by name.")]
        public void TestFindGroup()
        {
            var division = new EntityId("EmployeeManagement/EmployeeDivision/Default:1");
            var group = _apiSvc.GetGroupAsync("TEST_GROUP", _token).Result;

            Assert.IsNotNull(group);
        }

        [Test(Description = "Tests ability to find the \"Groups\" system folder.")]
        public void TestGetGroupsFolder()
        {
            var group = _apiSvc.GetGroupsFolderAsync(_token).Result;

            Assert.IsNotNull(group);
        }

        [Test]
        public void TestGetEmployeeId()
        {
            var employeeNumber = "12345";

            var employee = _apiSvc.GetEmployeeIdAsync(employeeNumber, _token).Result;

            Assert.IsNotNull(employee);
        }

        [Test]
        public void TestGetGroupMembership()
        {
            // fetch the profile id of admin employee and then use it to get 
            // the group membership 
            var adminEmpId = new EntityId("EmployeeManagement/Employee/Default:1");

            var profileToken = _apiSvc.GetEmployeeProfileIdAsync(adminEmpId, _token).Result;
            Assert.IsNotNull(profileToken);
            var profileId = new EntityId(profileToken);

            var groupMembership = _apiSvc.GetGroupMembershipAsync(profileId, _token).Result;
            Assert.IsNotNull(groupMembership);
            Assert.Greater(groupMembership.Count, 0);
        }

        [Test]
        public void TestGetDefaultWorkflow()
        {
            var workflow = _apiSvc.GetDefaultWorkflowTemplateAsync(_token).Result;
            Assert.IsNotNull(workflow);
        }

        [Test]
        public void TestGetGroupName()
        {
            var everyoneGroupToken = "EmployeeManagement/EmployeeGroup/Default:1";
            var groupName = _apiSvc.GetGroupNameAsync(everyoneGroupToken, _token).Result;
            Assert.AreEqual("Everyone", groupName);
        }

        [Test]
        public void TestUpdateEmployeeProfile()
        {
            // fetch the profile id of admin employee and then use it to get 
            // the group membership 
            var adminEmpId = new EntityId("EmployeeManagement/Employee/Default:1");

            var profileToken = _apiSvc.GetEmployeeProfileIdAsync(adminEmpId, _token).Result;
            Assert.IsNotNull(profileToken);
            var profileId = new EntityId(profileToken);

            var groupsToAdd = new List<EntityId>();
            groupsToAdd.Add(new EntityId("EmployeeManagement/EmployeeGroup/Default:1"));

            var groupsToRemove = new List<EntityId>();

            var res = _apiSvc.UpdateEmployeeProfileAsync(profileId, groupsToAdd, groupsToRemove, _token).Result;
            Assert.IsTrue(res);
        }

        [Test]
        public void TestUpdateEmployee()
        {
            var adminEmpId = new EntityId("EmployeeManagement/Employee/Default:1");
            var employee = new MetaObject();
            employee["FirstName"] = "Administrator";

            var res = _apiSvc.UpdateEmployeeAsync(adminEmpId, employee, _token).Result;

            Assert.IsTrue(res);
        }
    }
}
