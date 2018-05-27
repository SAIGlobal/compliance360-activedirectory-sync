using System;
using System.Threading;
using System.Threading.Tasks;
using Compliance360.EmployeeSync.Library.Configuration;
using NLog;

namespace Compliance360.EmployeeSync.Library.Jobs
{
    /// <summary>
    ///     This class represents a "Job" in the sync service. All
    ///     job types should derive from SyncJob and should implement Execute()
    ///     for job specific functionality.
    /// </summary>
    public abstract class SyncJob : ISyncJob
    {
        private bool _isStopped;
        private Task _task;
        private CancellationToken _token;
        private CancellationTokenSource _tokenSource;

        protected SyncJob(ILogger logger, JobElement jobConfig)
        {
            Logger = logger;
            JobConfig = jobConfig;
        }

        protected ILogger Logger { get; }
        public JobElement JobConfig { get; }
        public Action<ISyncJob> Callback { get; set; }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Start()
        {
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;

            Task.Run(() =>
            {
                while (!_isStopped)
                {
                    _tokenSource.Token.ThrowIfCancellationRequested();

                    try
                    {
                        // perform the job activity
                        Execute();

                        // perform the callback if present
                        Callback?.Invoke(this);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, $"Error executing job [{JobConfig.Name}].");
                    }


                    // wait for the next interval
                    var waitTime = JobConfig.IntervalSeconds * 1000;

                    if (_token != null && !_token.IsCancellationRequested)
                    {
                        _token.WaitHandle.WaitOne(waitTime);
                    }
                }
            }, _token);
        }

        public void Stop()
        {
            _isStopped = true;
            _tokenSource?.Cancel();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _task?.Dispose();
                _task = null;

                _tokenSource?.Dispose();
                _tokenSource = null;
            }
        }

        protected virtual void Execute()
        {
            Logger.Debug("Executing Job");
        }
    }
}