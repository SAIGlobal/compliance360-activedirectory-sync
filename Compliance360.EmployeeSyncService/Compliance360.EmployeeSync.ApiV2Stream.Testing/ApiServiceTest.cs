using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Compliance360.EmployeeSync.ApiV2Stream;
using Compliance360.EmployeeSync.ApiV2Stream.Data;
using Compliance360.EmployeeSync.Library;
using Compliance360.EmployeeSync.Library.Configuration;
using Compliance360.EmployeeSync.Library.Data;
using Moq;
using NLog;
using NUnit.Framework;

namespace Compliance360.EmployeeSync.ApiV2Stream.Testing
{
    [TestFixture(Description = "Tests for the API Stream")]
    public class ApiV2ServiceTest
    {
        [Test(Description = "Tests the ability to create a department")]
        public void TestCreateDepartment()
        {
            var division = new Entity { Id = "EmployeeManagement/EmployeeDivision/Default:1" };
            var logger = new Mock<ILogger>();

            // create the mock response message
            var responseContent =
                ReadJsonContentResource(
                    "Compliance360.EmployeeSync.ApiV2Stream.Testing.Data.CreateDepartmentResponse.json");
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            };
        
            var httpClient = new Mock<IHttpClientHandler>();
            httpClient.Setup(h => h.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
                .Returns(Task.FromResult(responseMessage));

            var httpDataService = new HttpDataService(logger.Object, httpClient.Object);

            var apiService = new ApiService(logger.Object, httpDataService);

            var department = apiService.CreateDepartmentAsync("TEST_DEPARTMENT", division, "mock_token").Result;
            Assert.IsNotNull(department);
        }

        [Test(Description = "Tests the ability to find a department")]
        public void TestFindDepartment()
        {
            var division = new Entity { Id = "EmployeeManagement/EmployeeDivision/Default:1" };
            var logger = new Mock<ILogger>();

            // create the mock response message
            var responseContent =
                ReadJsonContentResource(
                    "Compliance360.EmployeeSync.ApiV2Stream.Testing.Data.FindDepartmentResponse.json");
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            };

            var httpClient = new Mock<IHttpClientHandler>();
            httpClient.Setup(h => h.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(responseMessage));

            var httpDataService = new HttpDataService(logger.Object, httpClient.Object);

            var apiService = new ApiService(logger.Object, httpDataService);

            var department = apiService.GetDepartmentAsync("TEST_DEPARTMENT", division, "mock_token").Result;

            Assert.AreEqual("EmployeeManagement/EmployeeDepartment/Default:1", department);
        }
       
        [Test(Description = "Tests ability to login using app.config stream settings.")]
        public void TestLoginSuccess()
        {
            var logger = new Mock<ILogger>();

            // create the mock response messages
            var responseContent =
                ReadJsonContentResource(
                    "Compliance360.EmployeeSync.ApiV2Stream.Testing.Data.HostResponse.json");
            var hostResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            };

            responseContent =
                ReadJsonContentResource(
                    "Compliance360.EmployeeSync.ApiV2Stream.Testing.Data.LoginResponse.json");
            var loginResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            };

            var httpClient = new Mock<IHttpClientHandler>();
            httpClient.Setup(h => h.GetAsync("/API/2.0/Security/OrganizationHost?organization=TEST_ORG"))
                .Returns(Task.FromResult(hostResponse));

            httpClient.Setup(h => h.PostAsync("/API/2.0/Security/Login", It.IsAny<HttpContent>()))
                .Returns(Task.FromResult(loginResponse));

            var httpDataService = new HttpDataService(logger.Object, httpClient.Object);

            var apiService = new ApiService(logger.Object, httpDataService);
            var token = apiService.LoginAsync("https://secure.compliance360.com", "TEST_ORG", "TEST_USER", "TEST_PASSWORD").Result;

            Assert.AreEqual("123i4JESNOqOnRND5L5lqJbWO1xV9jc3%2DDng09BacirEAwMCgvupQHWp%2BDnCh", token);
        }

        [Test(Description = "Test ability to logout")]
        public void TestLogout()
        {
            var logger = new Mock<ILogger>();

            var logoutResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };

            var httpClient = new Mock<IHttpClientHandler>();
            httpClient.Setup(h => h.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(logoutResponse));

            var httpDataService = new HttpDataService(logger.Object, httpClient.Object);

            var apiService = new ApiService(logger.Object, httpDataService);
            var token = apiService.LogoutAsync("TEST_TOKEN").Result;

            Assert.IsNotNull(token);
        }

        [Test(Description = "Tests ability to create a new job title.")]
        public void TestCreateJobTitle()
        {
            var division = new Entity { Id = "EmployeeManagement/EmployeeDivision/Default:1" };
            var logger = new Mock<ILogger>();

            var responseContent =
                ReadJsonContentResource(
                    "Compliance360.EmployeeSync.ApiV2Stream.Testing.Data.CreateJobTitleResponse.json");
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            };

            var httpClient = new Mock<IHttpClientHandler>();
            httpClient.Setup(h => h.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
                .Returns(Task.FromResult(responseMessage));

            var httpDataService = new HttpDataService(logger.Object, httpClient.Object);

            var apiService = new ApiService(logger.Object, httpDataService);

            var jobTitle = apiService.CreateJobTitleAsync("TEST_JOB_TITLE", division, "TEST_TOKEN").Result;

            Assert.AreEqual("Lookup/Employee/JobTitleId:1", jobTitle);
        }

        [Test(Description = "Tests ability to return job titles from the api")]
        public void TestGetJobTitle()
        {
            var division = new Entity { Id = "EmployeeManagement/EmployeeDivision/Default:1" };
            var logger = new Mock<ILogger>();

            var responseContent =
                ReadJsonContentResource(
                    "Compliance360.EmployeeSync.ApiV2Stream.Testing.Data.GetJobTitleResponse.json");
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            };

            var httpClient = new Mock<IHttpClientHandler>();
            httpClient.Setup(h => h.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(responseMessage));

            var httpDataService = new HttpDataService(logger.Object, httpClient.Object);

            var apiService = new ApiService(logger.Object, httpDataService);

            var jobTitle = apiService.GetJobTitleAsync("TEST_JOB_TITLE", division, "TEST_TOKEN").Result;

            Assert.AreEqual("Lookup/Employee/JobTitleId:1", jobTitle);
        }

        [Test(Description = "Tests ability to handle missing job title from the api")]
        public void TestGetMissingJobTitle()
        {
            var division = new Entity { Id = "EmployeeManagement/EmployeeDivision/Default:1" };
            var logger = new Mock<ILogger>();

            var responseContent =
                ReadJsonContentResource(
                    "Compliance360.EmployeeSync.ApiV2Stream.Testing.Data.GetMissingJobTitleResponse.json");
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            };

            var httpClient = new Mock<IHttpClientHandler>();
            httpClient.Setup(h => h.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(responseMessage));

            var httpDataService = new HttpDataService(logger.Object, httpClient.Object);

            var apiService = new ApiService(logger.Object, httpDataService);

            var jobTitle = apiService.GetJobTitleAsync("TEST_MISSING_JOB_TITLE", division, "TEST_TOKEN").Result;

            Assert.IsNull(jobTitle);
        }

        [Test]
        public void TestGetDivision()
        {
            var logger = new Mock<ILogger>();

            var responseContent =
                ReadJsonContentResource(
                    "Compliance360.EmployeeSync.ApiV2Stream.Testing.Data.GetDivisionResponse.json");
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            };

            var httpClient = new Mock<IHttpClientHandler>();
            httpClient.Setup(h => h.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(responseMessage));

            var httpDataService = new HttpDataService(logger.Object, httpClient.Object);

            var apiService = new ApiService(logger.Object, httpDataService);

            var divisionId = apiService.GetDivisionAsync("Main Division", "TEST_TOKEN").Result;

            Assert.AreEqual("EmployeeManagement/EmployeeDivision/Default:1", divisionId);
        }

        
        [Test]
        public void TestGetMissingDivision()
        {
            var logger = new Mock<ILogger>();

            var responseContent =
                ReadJsonContentResource(
                    "Compliance360.EmployeeSync.ApiV2Stream.Testing.Data.GetMissingDivisionResponse.json");
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            };

            var httpClient = new Mock<IHttpClientHandler>();
            httpClient.Setup(h => h.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(responseMessage));

            var httpDataService = new HttpDataService(logger.Object, httpClient.Object);

            var apiService = new ApiService(logger.Object, httpDataService);

            var divisionId = apiService.GetDivisionAsync("Main Division", "TEST_TOKEN").Result;

            Assert.IsNull(divisionId);
        }

        [Test(Description = "Tests ability to create a new group.")]
        public void TestCreateGroup()
        {
            var logger = new Mock<ILogger>();

            var groupFolderResponseContent =
                ReadJsonContentResource(
                    "Compliance360.EmployeeSync.ApiV2Stream.Testing.Data.GetGroupFolderResponse.json");
            var groupFolderMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(groupFolderResponseContent)
            };

            var responseContent =
                ReadJsonContentResource(
                    "Compliance360.EmployeeSync.ApiV2Stream.Testing.Data.CreateGroupResponse.json");
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            };

            var httpClient = new Mock<IHttpClientHandler>();

            httpClient.Setup(h => h.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(groupFolderMessage));

            httpClient.Setup(h => h.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
                .Returns(Task.FromResult(responseMessage));

            var httpDataService = new HttpDataService(logger.Object, httpClient.Object);

            var apiService = new ApiService(logger.Object, httpDataService);

            var group = apiService.CreateGroupAsync("TEST_GROUP", "TEST_TOKEN").Result;

            Assert.AreEqual("EmployeeManagement/EmployeeGroup/Default:1", group);
        }

        [Test(Description = "Tests ability to find an existing group by name.")]
        public void TestFindGroup()
        {
            var logger = new Mock<ILogger>();

            var responseContent =
                ReadJsonContentResource(
                    "Compliance360.EmployeeSync.ApiV2Stream.Testing.Data.GetGroupResponse.json");
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            };

            var httpClient = new Mock<IHttpClientHandler>();
            httpClient.Setup(h => h.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(responseMessage));

            var httpDataService = new HttpDataService(logger.Object, httpClient.Object);

            var apiService = new ApiService(logger.Object, httpDataService);

            var group = apiService.GetGroupAsync("TEST_GROUP", "TEST_TOKEN").Result;

            Assert.AreEqual("EmployeeManagement/EmployeeGroup/Default:1", group);
        }
        
        [Test]
        public void TestGetEmployeeId()
        {
            var logger = new Mock<ILogger>();

            var responseContent =
                ReadJsonContentResource(
                    "Compliance360.EmployeeSync.ApiV2Stream.Testing.Data.GetEmployeeResponse.json");
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            };

            var httpClient = new Mock<IHttpClientHandler>();
            httpClient.Setup(h => h.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(responseMessage));

            var httpDataService = new HttpDataService(logger.Object, httpClient.Object);

            var apiService = new ApiService(logger.Object, httpDataService);

            var employeeId = apiService.GetEmployeeIdAsync("TEST_EMPLOYEE_NUM", "TEST_TOKEN").Result;

            Assert.AreEqual("EmployeeManagement/Employee/Default:1", employeeId);
        }

        [Test]
        public void TestGetGroupMembership()
        {
            var logger = new Mock<ILogger>();

            var responseContent =
                ReadJsonContentResource(
                    "Compliance360.EmployeeSync.ApiV2Stream.Testing.Data.GetGroupMembershipResponse.json");
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            };

            var httpClient = new Mock<IHttpClientHandler>();
            httpClient.Setup(h => h.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(responseMessage));

            var httpDataService = new HttpDataService(logger.Object, httpClient.Object);

            var apiService = new ApiService(logger.Object, httpDataService);
            
            var profile = new Entity{ Id = "EmployeeManagement/EmployeeProfile/Default:1" };
            var groupMembership = apiService.GetGroupMembershipAsync(profile, "TEST_TOKEN").Result;

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(groupMembership);
                Assert.Greater(groupMembership.Count, 0);
                Assert.AreEqual("EmployeeManagement/EmployeeGroup/Default:1", groupMembership[0]);
            });
        }
        
        [Test]
        public void TestGetDefaultWorkflow()
        {
            var logger = new Mock<ILogger>();

            var responseContent =
                ReadJsonContentResource(
                    "Compliance360.EmployeeSync.ApiV2Stream.Testing.Data.GetDefaultWorkflowResponse.json");
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            };

            var httpClient = new Mock<IHttpClientHandler>();
            httpClient.Setup(h => h.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(responseMessage));

            var httpDataService = new HttpDataService(logger.Object, httpClient.Object);

            var apiService = new ApiService(logger.Object, httpDataService);

            var workflow = apiService.GetDefaultWorkflowTemplateAsync("TEST_TOKEN").Result;
            
            Assert.AreEqual("Global/WorkflowTemplates/Employee:1", workflow);
        }

        [Test]
        public void TestGetGroupName()
        {
            var logger = new Mock<ILogger>();

            var responseContent =
                ReadJsonContentResource(
                    "Compliance360.EmployeeSync.ApiV2Stream.Testing.Data.GetGroupNameResponse.json");
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            };

            var httpClient = new Mock<IHttpClientHandler>();
            httpClient.Setup(h => h.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(responseMessage));

            var httpDataService = new HttpDataService(logger.Object, httpClient.Object);

            var apiService = new ApiService(logger.Object, httpDataService);

            var everyoneGroupToken = "EmployeeManagement/EmployeeGroup/Default:1";
            var groupName = apiService.GetGroupNameAsync(everyoneGroupToken, "TEST_TOKEN").Result;
            Assert.AreEqual("Everyone", groupName);
        }

        [Test]
        public void TestUpdateEmployeeProfile()
        {
            var logger = new Mock<ILogger>();
            var httpClient = new Mock<IHttpClientHandler>();

            var profileResponseContent =
                ReadJsonContentResource(
                    "Compliance360.EmployeeSync.ApiV2Stream.Testing.Data.GetProfileIdResponse.json");
            var profileResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(profileResponseContent)
            };
            httpClient.Setup(h => h.GetAsync("/API/2.0/Data/EmployeeManagement/Employee/Default?select=Profile&where=InstanceId='EmployeeManagement/Employee/Default:1'&token=TEST_TOKEN"))
                .Returns(Task.FromResult(profileResponseMessage));
            
            var updateResponseContent =
                ReadJsonContentResource(
                    "Compliance360.EmployeeSync.ApiV2Stream.Testing.Data.UpdateProfileResponse.json");
            var updateProfileResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(updateResponseContent)
            };
            httpClient.Setup(h => h.PostAsync("/API/2.0/Data/EmployeeManagement/EmployeeProfile/Default/1?token=TEST_TOKEN", It.IsAny<HttpContent>()))
                .Returns(Task.FromResult(updateProfileResponseMessage));
            
            var httpDataService = new HttpDataService(logger.Object, httpClient.Object);

            var apiService = new ApiService(logger.Object, httpDataService);
            
            var empToUpdate = new Entity { Id = "EmployeeManagement/Employee/Default:1" };

            var profileToken = apiService.GetEmployeeProfileIdAsync(empToUpdate, "TEST_TOKEN").Result;
            Assert.AreEqual("EmployeeManagement/EmployeeProfile/Default:1", profileToken);
            var profile = new Profile { Id = profileToken };

            var groupsToAdd = new List<Entity>();
            groupsToAdd.Add(new Entity { Id = "EmployeeManagement/EmployeeGroup/Default:1" });

            var groupsToRemove = new List<Entity>();
            groupsToRemove.Add(new Entity { Id = "EmployeeManagement/EmployeeGroup/Default:2" });
            
            var res = apiService.UpdateEmployeeProfileAsync(profile, groupsToAdd, groupsToRemove, "TEST_TOKEN").Result;
            Assert.IsTrue(res);
        }

        /**
        [Test]
        public void TestUpdateEmployee()
        {
            var adminEmpId = new EntityId("EmployeeManagement/Employee/Default:1");
            var employee = new MetaObject();
            employee["FirstName"] = "Administrator";

            var res = _apiSvc.UpdateEmployeeAsync(adminEmpId, employee, _token).Result;

            Assert.IsTrue(res);
        }
        **/

        public string ReadJsonContentResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
