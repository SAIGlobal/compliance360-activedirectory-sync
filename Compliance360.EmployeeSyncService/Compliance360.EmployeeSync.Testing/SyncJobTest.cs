using System.Threading;
using Compliance360.EmployeeSync.Library;
using Compliance360.EmployeeSync.Library.Configuration;
using Compliance360.EmployeeSync.Library.Jobs;
using NLog;
using NUnit.Framework;

namespace Compliance360.EmployeeSync.Testing
{
    [TestFixture]
    public class SyncJobTest
    {
        private class TestSyncJob : SyncJob
        {
            public TestSyncJob(ILogger logger, JobElement jobConfig) : base(logger, jobConfig)
            {
            }

            protected override void Execute()
            {
                Logger.Debug("Test Job Execution");
            }
        }

        [Test]
        public void TestStartSyncJob()
        {
            var container = ContainerFactory.GetContainer();
            var logger = container.GetInstance<ILogger>();
            var configService = container.GetInstance<IConfigurationService>();
            var config = configService.GetConfig();
            var loopCount = 0;
            var waitEvent = new ManualResetEvent(false);

            var job = new TestSyncJob(logger, config.Jobs[0]);
            job.Callback = j =>
            {
                // job made the callback...increment a counter
                // when two jobs complete...signal to stop processing
                loopCount++;

                if (loopCount == 2)
                    waitEvent.Set();
            };

            job.Start();

            var handles = new[] {waitEvent};
            WaitHandle.WaitAll(handles);

            // clean up the job
            job.Stop();
            job.Dispose();
        }
    }
}