using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Compliance360.EmployeeSync.ApiV2Stream.Data;
using Compliance360.EmployeeSync.ApiV2Stream.Services;
using Moq;
using NLog;
using NUnit.Framework;

namespace Compliance360.EmployeeSync.ApiV2Stream.Testing.Services
{
    [TestFixture(Description = "Tests for the Lookup Service")]
    public class LookupServiceTests
    {
        [Test(Description = "Tests the ability to create a Lookup")]
        public void TestCreateLookup()
        {
            var logger = new Mock<ILogger>();

            // create the mock response message
            var responseContent =
                ReadJsonContentResource(
                    "Compliance360.EmployeeSync.ApiV2Stream.Testing.Data.CreateLookupResponse.json");
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            };

            var httpClient = new Mock<IHttpClientHandler>();
            httpClient.Setup(h => h.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
                .Returns(Task.FromResult(responseMessage));

            var httpDataService = new HttpDataService(logger.Object, httpClient.Object);

            var apiService = new LookupService(logger.Object, httpDataService);

            var lookup = apiService.CreateLookupValue("TEST_LOOKUP_FIELD", "TEST_LOOKUP_VALUE", "TEST_TOKEN");

            Assert.AreEqual("Lookup/Employee/JobTitleId:1", lookup.Id);
        }

        [Test(Description = "Tests the ability to get a lookup")]
        public void TestGetLookup()
        {
            var logger = new Mock<ILogger>();

            // create the mock response message
            var responseContent =
                ReadJsonContentResource(
                    "Compliance360.EmployeeSync.ApiV2Stream.Testing.Data.GetLookupResponse.json");
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            };

            var httpClient = new Mock<IHttpClientHandler>();
            httpClient.Setup(h => h.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(responseMessage));

            var httpDataService = new HttpDataService(logger.Object, httpClient.Object);

            var apiService = new LookupService(logger.Object, httpDataService);

            var lookup = apiService.GetLookupValue("TEST_LOOKUP_FIELD", "TEST_LOOKUP_VALUE", "TEST_TOKEN");

            Assert.AreEqual("Lookup/Employee/JobTitleId:1", lookup.Id);
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
