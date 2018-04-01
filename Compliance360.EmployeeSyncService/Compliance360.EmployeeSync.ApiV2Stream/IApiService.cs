using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Compliance360.EmployeeSync.ApiV2Stream
{
    public interface IApiService
    {
        Task<string> CreateDepartmentAsync(string departmentName, EntityId division, string token);
        Task<string> CreateEmployeeAsync(MetaObject employee, string token);
        Task<string> CreateGroupAsync(string groupName, string token);
        Task<string> CreateJobTitleAsync(string jobTitle, EntityId division, string token);
        Task<string> GetDefaultWorkflowTemplateAsync(string token);
        Task<string> GetDepartmentAsync(string departmentName, EntityId division, string token);
        Task<string> GetDivisionAsync(string divisionName, string token);
        Task<string> GetEmployeeIdAsync(string userName, string token);
        Task<string> GetEmployeeProfileIdAsync(EntityId employeeId, string AuthToken);
        Task<string> GetGroupAsync(string groupName, string token);
        Task<string> GetGroupsFolderAsync(string token);
        Task<List<string>> GetGroupMembershipAsync(EntityId profileId, string token);
        Task<string> GetGroupNameAsync(string groupId, string token);
        Task<string> GetJobTitleAsync(string name, EntityId division, string token);
        Task<string> GetHostAddressAsync(string organization);
        Task<string> LoginAsync(string baseAddress, string organization, string username, string password);
        Task<bool> LogoutAsync(string token);
        Task<bool> UpdateEmployeeAsync(EntityId employeeId, MetaObject employee, string token);
        Task<bool> UpdateEmployeeProfileAsync(EntityId profileId, List<EntityId> groupsToAdd, List<EntityId> groupsToRemove, string token);
    }
}
