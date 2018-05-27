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
    [TestFixture(Description = "Tests for the API Stream")]
    public class GroupServiceTests
    {
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

            var groupService = new GroupService(logger.Object, httpDataService);

            var group = groupService.CreateGroupAsync("TEST_GROUP", "TEST_TOKEN").Result;

            Assert.AreEqual("EmployeeManagement/EmployeeGroup/Default:1", group.Id);
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

            var groupService = new GroupService(logger.Object, httpDataService);

            var group = groupService.GetGroupByName("TEST_GROUP", "TEST_TOKEN");

            Assert.AreEqual("EmployeeManagement/EmployeeGroup/Default:1", group.Id);
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

            var groupService = new GroupService(logger.Object, httpDataService);
            
            var profile = new Entity{ Id = "EmployeeManagement/EmployeeProfile/Default:1" };
            var groupMembership = groupService.GetGroupMembership(profile, "TEST_TOKEN");

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(groupMembership);
                Assert.Greater(groupMembership.Count, 0);
                Assert.AreEqual("EmployeeManagement/EmployeeGroup/Default:1", groupMembership[0].Id);
            });
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

            var groupService = new GroupService(logger.Object, httpDataService);

            var everyoneGroupToken = new Entity {Id = "EmployeeManagement/EmployeeGroup/Default:1"};

            var groupName = groupService.GetGroupName(everyoneGroupToken, "TEST_TOKEN");

            Assert.AreEqual("Everyone", groupName);
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
