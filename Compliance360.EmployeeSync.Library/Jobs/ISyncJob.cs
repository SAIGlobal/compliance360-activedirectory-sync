using System;
using Compliance360.EmployeeSync.Library.Configuration;

namespace Compliance360.EmployeeSync.Library.Jobs
{
    public interface ISyncJob : IDisposable
    {
        Action<ISyncJob> Callback { get; set; }
        JobElement JobConfig { get; }
        void Start();
        void Stop();
    }
}