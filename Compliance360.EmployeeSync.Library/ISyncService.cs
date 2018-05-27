namespace Compliance360.EmployeeSync.Library
{
    public interface ISyncService
    {
        /// <summary>
        ///     Called by the service manager to start the service.
        /// </summary>
        bool Start();

        /// <summary>
        ///     Called by the service manager to stop the service.
        /// </summary>
        bool Stop();
    }
}