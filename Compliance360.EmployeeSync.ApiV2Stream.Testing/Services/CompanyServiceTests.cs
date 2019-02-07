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
    [TestFixture(Description = "Tests for the Company Service")]
    public class CompanyServiceTests
    {
        [Test(Description = "Tests the ability to create a Company")]
        public void TestCreateCompany()
        {
            var logger = new Mock<ILogger>();

            // create the mock response message
            var responseContent =
                ReadJsonContentResource(
                    "Compliance360.EmployeeSync.ApiV2Stream.Testing.Data.CreateCompanyResponse.json");
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            };

            var httpClient = new Mock<IHttpClientHandler>();
            httpClient.Setup(h => h.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
                .Returns(Task.FromResult(responseMessage));

            var httpDataService = new HttpDataService(logger.Object, httpClient.Object);

            var apiService = new CompanyService(logger.Object, httpDataService);

            var company = apiService.CreateCompany("TEST_COMPANY_NAME", "TEST_TOKEN");

            Assert.AreEqual("EmployeeManagement/EmployeeCompany/Default:1", company.Id);
        }

        [Test(Description = "Tests the ability to get a Company")]
        public void TestGetCompany()
        {
            var logger = new Mock<ILogger>();

            // create the mock response message
            var responseContent =
                ReadJsonContentResource(
                    "Compliance360.EmployeeSync.ApiV2Stream.Testing.Data.GetCompanyResponse.json");
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            };

            var httpClient = new Mock<IHttpClientHandler>();
            httpClient.Setup(h => h.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(responseMessage));

            var httpDataService = new HttpDataService(logger.Object, httpClient.Object);

            var apiService = new CompanyService(logger.Object, httpDataService);

            var company = apiService.GetCompany("TEST_COMPANY_NAME", "TEST_TOKEN");

            Assert.AreEqual("EmployeeManagement/EmployeeCompany/Default:1", company.Id);
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
