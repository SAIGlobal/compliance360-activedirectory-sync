using System.Collections.Generic;
using System.Threading;
using Compliance360.EmployeeSync.Library;
using Compliance360.EmployeeSync.Library.Configuration;
using Compliance360.EmployeeSync.Library.Data;
using Compliance360.EmployeeSync.Library.Jobs;
using Moq;
using NUnit.Framework;

namespace Compliance360.EmployeeSync.Testing
{
    [TestFixture]
    public class ActiveDirectorySyncJobTest
    {
        [TearDown]
        public void Cleanup()
        {
            ContainerFactory.Reset();
        }

        [Test(Description = "Integration test for running the full version of the sync job.")]
        [Explicit]
        public void TestActiveDirectorySyncJobIntegration()
        {
            var container = ContainerFactory.GetContainer();
            var waitEvent = new ManualResetEvent(false);
            var configSvc = container.GetInstance<IConfigurationService>();
            var jobConfig = configSvc.GetConfig().Jobs[0];

            using (var activeDirectoryJob = container.With("jobConfig").EqualTo(jobConfig)
                .GetInstance<ISyncJob>("ActiveDirectory"))
            {
                activeDirectoryJob.Callback = j => { waitEvent.Set(); };

                activeDirectoryJob.Start();

                var handles = new[] {waitEvent};
                WaitHandle.WaitAll(handles);

                // clean up the job
                activeDirectoryJob.Stop();
            }
        }

        [Test]
        public void TestActiveDirectorySyncJob()
        {
            var job = new JobElement();
            job.Attributes.Add("sAMAccountName");
            job.Attributes.Add("manager");
            job.Attributes.Add("department");
            job.Attributes.Add("givenName");
            job.Attributes.Add("sn");

            // job has to have at least one output stream
            job.OutputStreams.Add("Logger");

            var adUser = new ActiveDirectoryUser();
            adUser.Attributes["sAMAccountName"] = "leetho0";
            adUser.Attributes["manager"] = "Mike Gilbert";
            adUser.Attributes["department"] = "development";
            adUser.Attributes["givenName"] = "Thomas";
            adUser.Attributes["sn"] = "Lee";
            
            var activeDirectoryServiceMock = new Mock<IActiveDirectoryService>();
            activeDirectoryServiceMock.Setup(service => service.GetActiveDirectoryUsers(job))
                .Returns(new List<ActiveDirectoryUser> {adUser});

            var container = ContainerFactory.GetContainer();
            container.Configure(c => c.For<IActiveDirectoryService>().Use(() => activeDirectoryServiceMock.Object));
            //container.Configure(c => c.For<ISftpService>().Use(() => sftpServiceMock.Object));

            var waitEvent = new ManualResetEvent(false);
            
            using (var activeDirectoryJob = container.With("jobConfig").EqualTo(job)
                .GetInstance<ISyncJob>("ActiveDirectory"))
            {
                activeDirectoryJob.Callback = j => { waitEvent.Set(); };

                activeDirectoryJob.Start();

                var handles = new[] { waitEvent };
                WaitHandle.WaitAll(handles);

                // clean up the job
                activeDirectoryJob.Stop();
            }
        }
    }
}