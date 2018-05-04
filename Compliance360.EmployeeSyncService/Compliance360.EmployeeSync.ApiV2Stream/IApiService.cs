using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compliance360.EmployeeSync.ApiV2Stream.Data;
using Newtonsoft.Json.Linq;

namespace Compliance360.EmployeeSync.ApiV2Stream
{
    public interface IApiService
    {
        Task<string> CreateDepartmentAsync(string departmentName, Entity division, string token);
        Task<string> CreateEmployeeAsync(Employee employee, string token);
        Task<string> CreateGroupAsync(string groupName, string token);
        Task<string> CreateJobTitleAsync(string jobTitle, Entity division, string token);
        Task<string> GetDefaultWorkflowTemplateAsync(string token);
        Task<string> GetDepartmentAsync(string departmentName, Entity division, string token);
        Task<string> GetDivisionAsync(string divisionName, string token);
        Task<string> GetEmployeeIdAsync(string userName, string token);
        Task<string> GetEmployeeProfileIdAsync(Entity employeeId, string AuthToken);
        Task<string> GetGroupAsync(string groupName, string token);
        Task<string> GetGroupsFolderAsync(string token);
        Task<List<string>> GetGroupMembershipAsync(Entity profileId, string token);
        Task<string> GetGroupNameAsync(string groupId, string token);
        Task<string> GetJobTitleAsync(string name, Entity division, string token);
        Task<string> GetHostAddressAsync(string organization);
        Task<string> LoginAsync(string baseAddress, string organization, string username, string password);
        Task<bool> LogoutAsync(string token);
        Task<bool> UpdateEmployeeAsync(Employee employee, string token);
        Task<bool> UpdateEmployeeProfileAsync(Profile profileId, List<Entity> groupsToAdd, List<Entity> groupsToRemove, string token);
    }
}
