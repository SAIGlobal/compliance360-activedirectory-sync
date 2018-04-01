using Compliance360.EmployeeSync.Library.Configuration;

namespace Compliance360.EmployeeSync.Library.Data
{
    public interface ISftpService
    {
        void SendFile(string filePath, 
            string host,
            string userName,
            string password,
            string destinationPath);
    }
}