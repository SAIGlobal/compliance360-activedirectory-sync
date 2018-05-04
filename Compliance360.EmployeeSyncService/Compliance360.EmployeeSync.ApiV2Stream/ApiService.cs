using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using Compliance360.EmployeeSync.Library.Data;
using Compliance360.EmployeeSync.ApiV2Stream.Data;

namespace Compliance360.EmployeeSync.ApiV2Stream
{
    /// <summary>
    /// This class is responsible for making the API calls to the
    /// Compliance 360 API. Please note that for performance considerations
    /// this class will maintain a reference to the HttpClient once LoginAsync()
    /// is called so that new HTTP sessions do not have to be established for 
    /// each call.
    /// </summary>
    public class ApiService : IApiService
    {
        private ILogger Logger { get; }
        private IHttpDataService Http { get; }
        private string _groupsFolderId = null;

        /// <summary>
        /// Initializes a new instance of the APIv2Service
        /// </summary>
        /// <param name="logger">Reference to a logger instance.</param>
        /// <param name="http">Reference to the HttpClient instance.</param>
        public ApiService(ILogger logger, IHttpDataService http)
        {
            Logger = logger;
            Http = http;
        }

        /// <summary>
        /// Creates a new department
        /// </summary>
        /// <param name="departmentName">The name of the department</param>
        /// <param name="division">The division where the department will be created</param>
        /// <param name="token">Current auth token</param>
        /// <returns>String id of the new department</returns>
        public async Task<string> CreateDepartmentAsync(string departmentName, Entity division, string token)
        {
            Logger.Debug("Creating department [{0}]", departmentName);

            var createDepartmentUri = $"/API/2.0/Data/EmployeeManagement/EmployeeDepartment/Default?token={token}";

            var department = new Dictionary<string, object>
            {
                { "DeptNum", departmentName },
                { "DeptName", departmentName },
                { "Division", division }
            };

            var result = await Http.PostAsync<CreateResponse>(createDepartmentUri, department);

            return result.Id;
        }

        /// <summary>
        /// Creates a new employee based on the supplied employee object
        /// </summary>
        /// <param name="employee">Objective describing the employee to create.</param>
        /// <param name="token">The active AuthToken.</param>
        /// <returns>String employee id</returns>
        public async Task<string> CreateEmployeeAsync(Employee employee, string token)
        {
            Logger.Debug("Creating employee");

            var createEmployeeUri = $"/API/2.0/Data/EmployeeManagement/Employee/Default?token={token}";
            
            // ensure required fields are present
            if (!employee.ContainsKey("FirstName") || string.IsNullOrEmpty(employee["FirstName"] as string))
            {
                employee["FirstName"] = "-";
            }
            if (!employee.ContainsKey("LastName") || string.IsNullOrEmpty(employee["LastName"] as string))
            {
                employee["LastName"] = "-";
            }
            if (!employee.ContainsKey("EmployeeNum") || string.IsNullOrEmpty(employee["EmployeeNum"] as string))
            {
                var empData = JsonConvert.SerializeObject(employee);
                throw new DataException($"EmployeeNum is missing and is required:\n{empData}");
            }

            var createEmployeeRequest = new Dictionary<string, object>();
            foreach (var key in employee.Keys.Where(k => k != "id"))
            {
                createEmployeeRequest[key] = employee[key];
            }

            var result = await Http.PostAsync<CreateResponse>(createEmployeeUri, createEmployeeRequest);

            return result.Id;
        }

        /// <summary>
        /// Creates a new group
        /// </summary>
        /// <param name="groupName">Name of the new group.</param>
        /// <param name="token">Auth token.</param>
        /// <returns>String id of the new group.</returns>
        public async Task<string> CreateGroupAsync(string groupName, string token)
        {
            Logger.Debug("Creating group [{0}]", groupName);

            if (_groupsFolderId == null)
            {
                _groupsFolderId = await this.GetGroupsFolderAsync(token);
            }

            var folder = new Dictionary<string, string>
            {
                { "id", _groupsFolderId }
            };

            var createGroupUri = $"/API/2.0/Data/EmployeeManagement/EmployeeGroup/Default?token={token}";
            var group = new Dictionary<string, object>
            {
                {"GroupName", groupName},
                {"Dynamic", false},
                {"UseForSecurity", true},
                {"UseForWorkflow", true},
                {"Folder", folder }
            };

            var result = await Http.PostAsync<CreateResponse>(createGroupUri, group);

            return result.Id;
        }

        /// <summary>
        /// Creates a new job title in the 
        /// </summary>
        /// <param name="jobTitleName">The job title to create.</param>
        /// <param name="division">The id of the division.</param>
        /// <param name="token">The current auth token.</param>
        /// <returns></returns>
        public async Task<string> CreateJobTitleAsync(string jobTitleName, Entity division, string token)
        {
            Logger.Debug("Creating Job Title [{0}]", jobTitleName);

            var createJobTitletUri = $"/API/2.0/Data/Lookup/Employee/JobTitleId?token={token}";

            var jobTitle = new Dictionary<string, object>
            {
                {"Text", jobTitleName}
            };

            var result = await Http.PostAsync<CreateResponse>(createJobTitletUri, jobTitle);

            return result.Id;
        }
        
        /// <summary>
        /// Gets the id of the default workflow template.
        /// </summary>
        /// <param name="token">The current auth token.</param>
        /// <returns></returns>
        public async Task<string> GetDefaultWorkflowTemplateAsync(string token)
        {
            Logger.Debug("Getting default Workflow");

            var getWorkflowTemplateUri =
                $"/API/2.0/Data/Global/WorkflowTemplates/Employee?take=1&where=IsDefault='True'&token={token}";

            var resp = await Http.GetAsync<GetResponse<Entity>>(getWorkflowTemplateUri);

            return resp.Data?.FirstOrDefault()?.Id;
        }

        /// <summary>
        /// Gets a department based on its name
        /// </summary>
        /// <param name="departmentName">The name of the department to find.</param>
        /// <param name="division">The id of the division which should contain the department.</param>
        /// <param name="token">The current active auth token.</param>
        /// <returns>String Id of the department.</returns>
        public async Task<string> GetDepartmentAsync(
            string departmentName, 
            Entity division, 
            string token)
        {
            Logger.Debug("Getting department [{0}]", departmentName);

            var where =
                $"((DeptNum='{Uri.EscapeDataString(departmentName)}')|(DeptName='{Uri.EscapeDataString(departmentName)}'))";
            
            var findDepartmentUri =
                $"/API/2.0/Data/EmployeeManagement/EmployeeDepartment/Default?take=1&where={where}&token={token}";

            var resp = await Http.GetAsync<GetResponse<Entity>>(findDepartmentUri);
            
            return resp.Data?.FirstOrDefault()?.Id;
        }

        /// <summary>
        /// Gets a Division based on the supplied division path.
        /// </summary>
        /// <param name="divisionPath">The Path of the division</param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<string> GetDivisionAsync(string divisionPath, string token)
        {
            Logger.Debug("Getting division [{0}]", divisionPath);

            var findDivisionUri =
                $"/API/2.0/Data/EmployeeManagement/EmployeeDivision/Default?take=1&where=Path='{Uri.EscapeDataString(divisionPath)}'&token={token}";

            var resp = await Http.GetAsync<GetResponse<Entity>>(findDivisionUri);

            return resp.Data?.FirstOrDefault()?.Id;
        }

        /// <summary>
        /// Gets an employee based on the supplied username.
        /// </summary>
        /// <param name="employeeNum">The employee number (unique id) of the employee to find.</param>
        /// <param name="token">The active AuthToken.</param>
        /// <returns>String employee id</returns>
        public async Task<string> GetEmployeeIdAsync(string employeeNum, string token)
        {
            Logger.Debug("Getting Id of Employee using EmployeeNumber [{0}]", employeeNum);

            var findEmployeeUri = $"/API/2.0/Data/EmployeeManagement/Employee/Default?take=1&where=EmployeeNum='{Uri.EscapeDataString(employeeNum)}'&token={token}";

            var resp = await Http.GetAsync<GetResponse<Entity>>(findEmployeeUri);

            return resp.Data?.FirstOrDefault()?.Id;
        }

        /// <summary>
        /// Gets the local profile id for the specified user.
        /// </summary>
        /// <param name="employeeId">Id of the current employee</param>
        /// <param name="token">Auth token</param>
        /// <returns>String identifier of the employee's local profile</returns>
        public async Task<string> GetEmployeeProfileIdAsync(Entity employeeId, string token)
        {
            Logger.Debug("Getting Profile for Employee [{0}]", employeeId);

            var getEmployeeProfileUri = $"/API/2.0/Data/EmployeeManagement/Employee/Default?select=Profile&where=InstanceId='{employeeId.Id}'&token={token}";

            var resp = await Http.GetAsync<GetResponse<GetEmployeeProfileIdResponse>>(getEmployeeProfileUri);

            return resp.Data?.FirstOrDefault()?.Profile.Id;
        }

        /// <summary>
        /// Gets a group based on its name
        /// </summary>
        /// <param name="groupName">Name of the group to find.</param>
        /// <param name="token">Current auth token.</param>
        /// <returns></returns>
        public async Task<string> GetGroupAsync(string groupName, string token)
        {
            Logger.Debug("Getting Group [{0}]", groupName);

            var getGroupUri =
                $"/API/2.0/Data/EmployeeManagement/EmployeeGroup/Default?where=GroupName='{Uri.EscapeDataString(groupName)}'&take=1&token={token}";

            var resp = await Http.GetAsync<GetResponse<Entity>>(getGroupUri);

            return resp.Data?.FirstOrDefault()?.Id;
        }

        /// <summary>
        /// Gets the Id of the "Groups" folder where the
        /// groups will be created.
        /// </summary>
        /// <param name="token">Auth token</param>
        /// <returns>Id of the Groups folder</returns>
        public async Task<string> GetGroupsFolderAsync(string token)
        {
            Logger.Debug("Getting \"Groups\" root folder");

            var getGroupsFolderUri =
                $"/API/2.0/Data/Global/Folders/Default?select=Name,Parent,Division&where=Name='Groups'&token={token}";

            var resp = await Http.GetAsync<GetResponse<Folder>>(getGroupsFolderUri);
            
            var folder = resp.Data?.FirstOrDefault(f => f.Parent.Id == "NULL" && f.Division.Id == "NULL");
            return folder?.Id;
        }

        /// <summary>
        /// Returns a dictionary of groups for which the user is a member.
        /// </summary>
        /// <param name="profileId">The local profile id.</param>
        /// <param name="token">The auth token.</param>
        /// <returns></returns>
        public async Task<List<string>> GetGroupMembershipAsync(Entity profile, string token)
        {
            Logger.Debug("Getting Group Membership for [{0}]", profile.Id);

            var getGroupsUri =
                $"/API/2.0/Data/EmployeeManagement/EmployeeProfile/Default?select=Groups&where=InstanceId='{profile.Id}'&token={token}";

            var resp = await Http.GetAsync<GetResponse<Profile>>(getGroupsUri);

            if (resp.Data?.Count > 0)
            {
                var groups = new List<string>();
                foreach (var group in resp.Data[0].Groups)
                {
                    groups.Add(group.Id);
                }

                return groups;
            }

            return null;
        }

        /// <summary>
        /// Gets a group name based on its Id
        /// </summary>
        /// <param name="groupId">Id of the group to find.</param>
        /// <param name="token">Auth token</param>
        /// <returns>Name of the group</returns>
        public async Task<string> GetGroupNameAsync(string groupId, string token)
        {
            Logger.Debug("Getting Group Name for [{0}]", groupId);

            var getGroupUri =
                $"/API/2.0/Data/EmployeeManagement/EmployeeGroup/Default?select=GroupName&where=InstanceId='{groupId}'&take=1&token={token}";

            var resp = await Http.GetAsync<GetResponse<EmployeeGroup>>(getGroupUri);

            return resp.Data?.FirstOrDefault()?.GroupName;
        }

        /// <summary>
        /// Returns a job title by name
        /// </summary>
        /// <param name="name">Name of the job title.</param>
        /// <param name="division">The entity id of the division that contains the job title.</param>
        /// <param name="token">The auth token.</param>
        /// <returns>Job title id</returns>
        public async Task<string> GetJobTitleAsync(string name, Entity division, string token)
        {
            Logger.Debug("Getting Job Title [{0}]", name);

            var jobTitlesUri = $"/API/2.0/Data/Lookup/Employee/JobTitleId?select=Text&take=1&where=Text='{Uri.EscapeDataString(name)}'&token={token}";
            var resp = await Http.GetAsync<GetResponse<Entity>>(jobTitlesUri);
            
            return resp.Data?.FirstOrDefault()?.Id;
        }

        /// <summary>
        /// Get host address 
        /// </summary>
        /// <param name="organization">Organization name</param>
        /// <returns>Address of the API for the organization.</returns>
        public async Task<string> GetHostAddressAsync(string organization)
        {
            Logger.Debug("Getting Host Address for organization [{0}]", organization);

            // the compliance 360 api endpoint can change. make the call
            // to get the correct base address
            var orgHostUri = $"/API/2.0/Security/OrganizationHost?organization={Uri.EscapeDataString(organization)}";
            var resp = await Http.GetAsync<HostResponse>(orgHostUri);

            if (resp.Host == null)
            {
                throw new DataException($"Cannot get organization host address at: {orgHostUri}");
            }

            return resp.Host;
        }

        /// <summary>
        /// Logs into the C360 API. Returns the API auth token.
        /// </summary>
        /// <param name="baseAddress">The base uri of the api.</param>
        /// <param name="organization">The organization name.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public async Task<string> LoginAsync(string baseAddress, string organization, string username, string password)
        {
            Logger.Debug("Logging in to API Organization:{0} Username:{1}", organization, username);

            // init the http client
            Http.Initialize(baseAddress);
            
            // get the api host address based on the organization
            var hostAddress = await GetHostAddressAsync(organization);
            if (hostAddress != baseAddress)
            {
                Http.Initialize(hostAddress);
            }

            // make the request to authenticate with the api
            var loginData = new
            {
                organization,
                username,
                password,
                culture = "en-US"
            };

            const string loginUri = "/API/2.0/Security/Login";

            var resp = await Http.PostAsync<LoginResponse>(loginUri, loginData);
            
            return resp.Token;
        }

        /// <summary>
        /// Logs the user out of the C360 application terminating the session.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<bool> LogoutAsync(string token)
        {
            Logger.Debug("Logging out of API");

            var logoutUri = $"/API/2.0/Security/Logout?token={Uri.EscapeUriString(token)}";
            await Http.GetAsync(logoutUri);

            return true;
        }

        /// <summary>
        /// Updates an employee using the API
        /// </summary>
        /// <param name="employeeId">Id token of the employee to update</param>
        /// <param name="employee">The employee metadata object</param>
        /// <param name="token">The current active auth token</param>
        /// <returns>Void</returns>
        public async Task<bool> UpdateEmployeeAsync(Employee employee, string token)
        {
            Logger.Debug("Updating Employee [{0}]", employee.Id);

            var createEmployeeUri = $"/API/2.0/Data/EmployeeManagement/Employee/Default/{employee.InstanceId}?token={token}";
            
            // ensure required fields are present
            if (employee.ContainsKey("FirstName") && string.IsNullOrEmpty(employee["FirstName"] as string))
            {
                employee["FirstName"] = "-";
            }
            if (employee.ContainsKey("LastName") && string.IsNullOrEmpty(employee["LastName"] as string))
            {
                employee["LastName"] = "-";
            }
            if (employee.ContainsKey("EmployeeNum") && string.IsNullOrEmpty(employee["EmployeeNum"] as string))
            {
                var empData = JsonConvert.SerializeObject(employee);
                throw new DataException($"EmployeeNum is missing and is required:\n{empData}");
            }

            var employeeUpdateRequest = new Dictionary<string, object>();
            foreach (var key in employee.Keys.Where(k => k != "id"))
            {
                employeeUpdateRequest[key] = employee[key];
            }
            
            var resp = await Http.PostAsync<CreateResponse>(createEmployeeUri, employeeUpdateRequest);
            
            return true;
        }

        /// <summary>
        /// Updates an employee profile to add/remove group membership
        /// </summary>
        /// <param name="profile">The profile to update.</param>
        /// <param name="groupsToAdd">List of groups to add to the user.</param>
        /// <param name="groupsToRemove">List of groups that should be removed from the user.</param>
        /// <param name="token">Current auth token.</param>
        /// <returns></returns>
        public async Task<bool> UpdateEmployeeProfileAsync(Profile profile, List<Entity> groupsToAdd, List<Entity> groupsToRemove, string token)
        {
            Logger.Debug("Updating Employee Profile [{0}]", profile.Id);

            if (groupsToAdd == null || groupsToAdd.Count == 0)
                return true;

            var updateProfileUri = $"/API/2.0/Data/EmployeeManagement/EmployeeProfile/Default/{profile.InstanceId}?token={token}";

            profile.Groups = new List<EntityReference>();
            
            groupsToAdd.ForEach(g =>
            {
                profile.Groups.Add(new EntityReference { Action = "Add", Id=g.Id});
            });

            groupsToRemove.ForEach(g =>
            {
                profile.Groups.Add(new EntityReference { Action = "Remove", Id = g.Id });
            });
            
            var groupData = JsonConvert.SerializeObject(profile);
            var content = new StringContent(groupData, Encoding.UTF8, "application/json");

            await Http.PostAsync<CreateResponse>(updateProfileUri, profile);
            
            return true;
        }
    }
}
