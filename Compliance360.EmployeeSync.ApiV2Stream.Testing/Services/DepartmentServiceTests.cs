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
    [TestFixture(Description = "Tests for the Department Service")]
    public class DepartmentServiceTest
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

            var apiService = new DepartmentService(logger.Object, httpDataService);

            var department = apiService.CreateDepartmentAsync("TEST_DEPARTMENT", division, "TEST_TOKEN").Result;

            Assert.AreEqual("EmployeeManagement/EmployeeDepartment/Default:1", department.Id);
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

            var apiService = new DepartmentService(logger.Object, httpDataService);

            var department = apiService.GetDepartmentAsync("TEST_DEPARTMENT", division, "mock_token").Result;

            Assert.AreEqual("EmployeeManagement/EmployeeDepartment/Default:1", department.Id);
        }

        [Test(Description = "Tests the ability to find a department")]
        public void TestFindMissingDepartment()
        {
            var division = new Entity { Id = "EmployeeManagement/EmployeeDivision/Default:1" };
            var logger = new Mock<ILogger>();

            // create the mock response message
            var responseContent =
                ReadJsonContentResource(
                    "Compliance360.EmployeeSync.ApiV2Stream.Testing.Data.FindDepartmentMissingResponse.json");
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            };

            var httpClient = new Mock<IHttpClientHandler>();
            httpClient.Setup(h => h.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(responseMessage));

            var httpDataService = new HttpDataService(logger.Object, httpClient.Object);

            var apiService = new DepartmentService(logger.Object, httpDataService);

            var department = apiService.GetDepartmentAsync("TEST_DEPARTMENT", division, "mock_token").Result;

            Assert.That(department, Is.Null);
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
