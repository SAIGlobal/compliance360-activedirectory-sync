using System;
using System.Threading;
using Compliance360.EmployeeSync.Library;
using Compliance360.EmployeeSync.Library.Configuration;
using Compliance360.EmployeeSync.Library.Jobs;
using Moq;
using NLog;
using NUnit.Framework;

namespace Compliance360.EmployeeSync.Testing
{
    [TestFixture]
    public class SyncServiceTest
    {
        [Test]
        public void RunTheSyncService()
        {
            var config = new SyncConfigurationSection();
            config.Jobs.Add("TestJob");
            var job = config.Jobs[0];
            job.Type = "TestJob";
            job.IntervalSeconds = 5;
            
            var configSvcMock = new Mock<IConfigurationService>();
            configSvcMock.Setup(svc => svc.GetConfig())
                .Returns(config);

            var startCount = 0;
            var syncJobMock = new Mock<ISyncJob>();
            syncJobMock.Setup(syncJob => syncJob.Start())
                .Callback(() => startCount++);

            var container = ContainerFactory.GetContainer();

            container.Configure(c =>
            {
                c.For<IConfigurationService>().Use(() => configSvcMock.Object);
                c.For<ISyncJob>().Use(syncJobMock.Object).Named("TestJob");
            });

            var syncService = container.GetInstance<ISyncService>();
            if (!syncService.Start())
                Assert.Fail("Could not start the sync service.");

            Thread.Sleep(3000);

            syncService.Stop();

            syncJobMock.Verify(syncJob => syncJob.Start(), Times.AtLeastOnce);
            syncJobMock.Verify(syncJob => syncJob.Stop(), Times.AtLeastOnce);
        }
    }
}