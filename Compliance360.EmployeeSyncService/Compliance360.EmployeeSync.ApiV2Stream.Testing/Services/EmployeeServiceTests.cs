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
using Compliance360.EmployeeSync.ApiV2Stream.Services;
using Compliance360.EmployeeSync.Library;
using Compliance360.EmployeeSync.Library.Configuration;
using Compliance360.EmployeeSync.Library.Data;
using Moq;
using NLog;
using NUnit.Framework;

namespace Compliance360.EmployeeSync.ApiV2Stream.Testing.Services
{
    [TestFixture(Description = "Tests for the Employee Service")]
    public class EmployeeServiceTests
    {
        [Test]
        public void TestGetEmployee()
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

            var employeeService = new EmployeeService(logger.Object, httpDataService);

            var employee = employeeService.GetEmployeeByEmployeeNum("TEST_EMPLOYEE_NUM", "TEST_TOKEN");

            Assert.AreEqual("EmployeeManagement/Employee/Default:1", employee.Id);
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

            var employeeService = new EmployeeService(logger.Object, httpDataService);

            var workflow = employeeService.GetDefaultWorkflowTemplate("TEST_TOKEN");
            
            Assert.AreEqual("Global/WorkflowTemplates/Employee:1", workflow.Id);
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

            var employeeService = new EmployeeService(logger.Object, httpDataService);
            
            var empToUpdate = new Entity { Id = "EmployeeManagement/Employee/Default:1" };

            var profileRef = employeeService.GetEmployeeProfile(empToUpdate, "TEST_TOKEN");
            Assert.AreEqual("EmployeeManagement/EmployeeProfile/Default:1", profileRef.Id);
            var profile = new Profile { Id = profileRef.Id };

            var groupsToAdd = new List<Entity>();
            groupsToAdd.Add(new Entity { Id = "EmployeeManagement/EmployeeGroup/Default:1" });

            var groupsToRemove = new List<Entity>();
            groupsToRemove.Add(new Entity { Id = "EmployeeManagement/EmployeeGroup/Default:2" });
            
            employeeService.UpdateEmployeeProfile(profile, groupsToAdd, groupsToRemove, "TEST_TOKEN");
        }

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
