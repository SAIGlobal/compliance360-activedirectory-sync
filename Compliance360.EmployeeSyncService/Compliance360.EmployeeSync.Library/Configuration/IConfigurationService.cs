namespace Compliance360.EmployeeSync.Library.Configuration
{
    public interface IConfigurationService
    {
        SyncConfigurationSection GetConfig();
    }
}