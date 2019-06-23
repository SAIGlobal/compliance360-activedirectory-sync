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
    [TestFixture(Description = "Tests for the Authenitcation Service")]
    public class AuthenticationServiceTest
    {
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

            httpClient.Setup(h => h.PostAsync("/API/2.0/Security/Authenticate", It.IsAny<HttpContent>()))
                .Returns(Task.FromResult(loginResponse));

            var httpDataService = new HttpDataService(logger.Object, httpClient.Object);

            var authService = new AuthenticationService(logger.Object, httpDataService);
            var token = authService.Authenticate("https://secure.compliance360.com", "TEST_ORG", "TEST_INTEGRATIONKEY", "TEST_CULTURE");

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

            var authService = new AuthenticationService(logger.Object, httpDataService);
            authService.Logout("TEST_TOKEN");
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
