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
    [TestFixture(Description = "Tests for the DivisionService")]
    public class DivisionServiceTests
    {
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

            var divisionService = new DivisionService(logger.Object, httpDataService);

            var division = divisionService.GetDivisionByName("Main Division", "TEST_TOKEN");

            Assert.AreEqual("EmployeeManagement/EmployeeDivision/Default:1", division.Id);
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

            var divisionService = new DivisionService(logger.Object, httpDataService);

            var division = divisionService.GetDivisionByName("Main Division", "TEST_TOKEN");

            Assert.IsNull(division);
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
