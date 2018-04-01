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
        private HttpClient Http { get; set; }
        private string _GroupsFolderId = null;

        /// <summary>
        /// Initializes a new instance of the APIv2Service
        /// </summary>
        /// <param name="logger">Reference to a logger instance.</param>
        public ApiService(ILogger logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Creates a new department
        /// </summary>
        /// <param name="departmentName">The name of the department</param>
        /// <param name="division">The division where the department will be created</param>
        /// <param name="token">Current auth token</param>
        /// <returns>String id of the new department</returns>
        public async Task<string> CreateDepartmentAsync(string departmentName, EntityId division, string token)
        {
            Logger.Debug("Creating department [{0}]", departmentName);

            var createDepartmentUri = $"/API/2.0/Data/EmployeeManagement/EmployeeDepartment/Default?token={token}";

            var department = new Dictionary<string, object>();
            department["DeptNum"] = departmentName;
            department["DeptName"] = departmentName;
            department["Division"] = division;

            var departmentData = JsonConvert.SerializeObject(department);
            var content = new StringContent(departmentData, Encoding.UTF8, "application/json");

            var resp = await Http.PostAsync(createDepartmentUri, content);
            if (!resp.IsSuccessStatusCode)
            {
                throw new DataException($"Error creating department using API:  ({resp.StatusCode}): {resp.ReasonPhrase}\n{departmentData}");
            }

            var respContent = await resp.Content.ReadAsStringAsync();
            var createResult = JObject.Parse(respContent);
            return createResult["id"].Value<string>();
        }

        /// <summary>
        /// Creates a new employee based on the supplied employee object
        /// </summary>
        /// <param name="employee">Objective describing the employee to create.</param>
        /// <param name="token">The active AuthToken.</param>
        /// <returns>String employee id</returns>
        public async Task<string> CreateEmployeeAsync(MetaObject employee, string token)
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

            var employeeData = JsonConvert.SerializeObject(employee);
            var createEmployeeContent = new StringContent(employeeData, Encoding.UTF8, "application/json");
            var resp = await Http.PostAsync(createEmployeeUri, createEmployeeContent);
            if (!resp.IsSuccessStatusCode)
            {
                throw new DataException($"Error creating employee using API:  ({resp.StatusCode}): {resp.ReasonPhrase}\n{employeeData}");
            }

            var content = await resp.Content.ReadAsStringAsync();
            var createEmployeeResult = JObject.Parse(content);
            return createEmployeeResult["id"].Value<string>();
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

            if (_GroupsFolderId == null)
            {
                _GroupsFolderId = await this.GetGroupsFolderAsync(token);
            }

            var folder = new Dictionary<string, string>
            {
                { "id", _GroupsFolderId }
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

            var groupData = JsonConvert.SerializeObject(group);
            var content = new StringContent(groupData, Encoding.UTF8, "application/json");

            var resp = await Http.PostAsync(createGroupUri, content);
            if (!resp.IsSuccessStatusCode)
            {
                throw new DataException($"Error creating Group API:  ({resp.StatusCode}): {resp.ReasonPhrase}\n{groupData}");
            }

            var respContent = await resp.Content.ReadAsStringAsync();
            var createResult = JObject.Parse(respContent);
            return createResult["id"].Value<string>();
        }

        /// <summary>
        /// Creates a new job title in the 
        /// </summary>
        /// <param name="jobTitleName">The job title to create.</param>
        /// <param name="division">The id of the division.</param>
        /// <param name="token">The current auth token.</param>
        /// <returns></returns>
        public async Task<string> CreateJobTitleAsync(string jobTitleName, EntityId division, string token)
        {
            Logger.Debug("Creating Job Title [{0}]", jobTitleName);

            var createJobTitletUri = $"/API/2.0/Data/Lookup/Employee/JobTitleId?token={token}";

            var jobTitle = new Dictionary<string, object>();
            jobTitle["Text"] = jobTitleName;

            var jobTitleData = JsonConvert.SerializeObject(jobTitle);
            var content = new StringContent(jobTitleData, Encoding.UTF8, "application/json");

            var resp = await Http.PostAsync(createJobTitletUri, content);
            if (!resp.IsSuccessStatusCode)
            {
                throw new DataException($"Error creating Job Title using API:  ({resp.StatusCode}): {resp.ReasonPhrase}\n{jobTitleData}");
            }

            var respContent = await resp.Content.ReadAsStringAsync();
            var createResult = JObject.Parse(respContent);
            return createResult["id"].Value<string>();
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
            var resp = await Http.GetAsync(getWorkflowTemplateUri);
            if (!resp.IsSuccessStatusCode)
            {
                throw new DataException($"Error getting employee workflow template using API: ({resp.StatusCode}): {resp.ReasonPhrase}");
            }

            var content = await resp.Content.ReadAsStringAsync();
            var workflowTemplateResult = JObject.Parse(content);

            if (workflowTemplateResult["data"].HasValues)
            {
                return workflowTemplateResult["data"][0]["id"].Value<string>();
            }

            return null;
        }

        /// <summary>
        /// Gets a department based on its name
        /// </summary>
        /// <param name="departmentName">The name of the department to find.</param>
        /// <param name="division">The id of the division which should contain the department.</param>
        /// <param name="token">The current active auth token.</param>
        /// <returns>String Id of the department.</returns>
        public async Task<string> GetDepartmentAsync(string departmentName, 
            EntityId division, 
            string token)
        {
            Logger.Debug("Getting department [{0}]", departmentName);

            var where =
                $"((DeptNum='{Uri.EscapeDataString(departmentName)}')|(DeptName='{Uri.EscapeDataString(departmentName)}'))";
            //$"((Division='{division.Token}')%26((DeptNum='{Uri.EscapeDataString(departmentName)}')|(DeptName='{Uri.EscapeDataString(departmentName)}')))";
            var findDepartmentUri =
                $"/API/2.0/Data/EmployeeManagement/EmployeeDepartment/Default?take=1&where={where}&token={token}";
            var resp = await Http.GetAsync(findDepartmentUri);
            if (!resp.IsSuccessStatusCode)
            {
                throw new DataException($"Error finding department [{departmentName}] using API:  ({resp.StatusCode}): {resp.ReasonPhrase}");
            }

            var content = await resp.Content.ReadAsStringAsync();
            var getDepartmentResult = JObject.Parse(content);

            if (getDepartmentResult["data"].HasValues)
            {
                return getDepartmentResult["data"][0]["id"].Value<string>();
            }

            return null;
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
            var resp = await Http.GetAsync(findDivisionUri);
            if (!resp.IsSuccessStatusCode)
            {
                throw new DataException($"Error finding division [{divisionPath}] using API:  ({resp.StatusCode}): {resp.ReasonPhrase}");
            }

            var content = await resp.Content.ReadAsStringAsync();
            var fndDivisionResult = JObject.Parse(content);

            if (fndDivisionResult["data"].HasValues)
            {
                return fndDivisionResult["data"][0]["id"].Value<string>();
            }

            return null;
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

            var resp = await Http.GetAsync(findEmployeeUri);
            if (!resp.IsSuccessStatusCode)
            {
                throw new DataException($"Error finding employee [{employeeNum}] using API:  ({resp.StatusCode}): {resp.ReasonPhrase}");
            }

            var content = await resp.Content.ReadAsStringAsync();
            var findEmployeeResult = JObject.Parse(content);

            if (findEmployeeResult["data"].HasValues)
            {
                return findEmployeeResult["data"][0]["id"].Value<string>();
            }

            return null;
        }

        /// <summary>
        /// Gets the local profile id for the specified user.
        /// </summary>
        /// <param name="employeeId">Id of the current employee</param>
        /// <param name="token">Auth token</param>
        /// <returns>String identifier of the employee's local profile</returns>
        public async Task<string> GetEmployeeProfileIdAsync(EntityId employeeId, string token)
        {
            Logger.Debug("Getting Profile for Employee [{0}]", employeeId);

            var getEmployeeProfileUri = $"/API/2.0/Data/EmployeeManagement/Employee/Default?select=Profile&where=InstanceId='{employeeId.Token}'&token={token}";

            var resp = await Http.GetAsync(getEmployeeProfileUri);
            if (!resp.IsSuccessStatusCode)
            {
                throw new DataException($"Error finding Employee Profile for [{employeeId.Token}] from API:  ({resp.StatusCode}): {resp.ReasonPhrase}");
            }

            var content = await resp.Content.ReadAsStringAsync();
            var jobTitleResult = JObject.Parse(content);

            if (jobTitleResult["data"].HasValues)
            {
                return jobTitleResult["data"][0]["Profile"]["id"].Value<string>();
            }

            return null;
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

            var resp = await Http.GetAsync(getGroupUri);
            if (!resp.IsSuccessStatusCode)
            {
                throw new DataException($"Error finding Group by name [{groupName}] from API: {getGroupUri} ({resp.StatusCode}): {resp.ReasonPhrase}");
            }

            var content = await resp.Content.ReadAsStringAsync();
            var groupResult = JObject.Parse(content);

            if (groupResult["data"].HasValues)
            {
                return groupResult["data"][0]["id"].Value<string>();
            }

            return null;
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

            var resp = await Http.GetAsync(getGroupsFolderUri);
            if (!resp.IsSuccessStatusCode)
            {
                throw new DataException($"Error finding Group Folder from API:  ({resp.StatusCode}): {resp.ReasonPhrase}");
            }

            var content = await resp.Content.ReadAsStringAsync();
            var folderResult = JObject.Parse(content);
            if (folderResult["data"].HasValues)
            {
                // loop through the folders and return the first
                // folder that has a null Parent and Division
                var folder = folderResult["data"].Children().FirstOrDefault(res =>
                    res["Parent"]["id"].Value<string>() == "NULL" && res["Division"]["id"].Value<string>() == "NULL");
                return folder?["id"].Value<string>();
            }

            return null;
        }

        /// <summary>
        /// Returns a dictionary of groups for which the user is a member.
        /// </summary>
        /// <param name="profileId">The local profile id.</param>
        /// <param name="token">The auth token.</param>
        /// <returns></returns>
        public async Task<List<string>> GetGroupMembershipAsync(EntityId profileId, string token)
        {
            Logger.Debug("Getting Group Membership for [{0}]", profileId.Token);

            var getGroupsUri =
                $"/API/2.0/Data/EmployeeManagement/EmployeeProfile/Default?select=Groups&where=InstanceId='{profileId.Token}'&token={token}";

            var resp = await Http.GetAsync(getGroupsUri);
            if (!resp.IsSuccessStatusCode)
            {
                throw new DataException($"Error finding groups for Profile [{profileId.Token}] from API:  ({resp.StatusCode}): {resp.ReasonPhrase}");
            }

            var content = await resp.Content.ReadAsStringAsync();
            var groupResult = JObject.Parse(content);

            if (groupResult["data"].HasValues)
            {
                var groups = new List<string>();
                foreach (var group in groupResult["data"][0]["Groups"].Children())
                {
                    groups.Add(group["id"].Value<string>());
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

            var resp = await Http.GetAsync(getGroupUri);
            if (!resp.IsSuccessStatusCode)
            {
                throw new DataException($"Error finding Group [{groupId}] from API:  ({resp.StatusCode}): {resp.ReasonPhrase}");
            }

            var content = await resp.Content.ReadAsStringAsync();
            var groupResult = JObject.Parse(content);

            if (groupResult["data"].HasValues)
            {
                return groupResult["data"][0]["GroupName"].Value<string>();
            }

            return null;
        }

        /// <summary>
        /// Returns a job title by name
        /// </summary>
        /// <param name="name">Name of the job title.</param>
        /// <param name="division">The entity id of the division that contains the job title.</param>
        /// <param name="token">The auth token.</param>
        /// <returns>Job title id</returns>
        public async Task<string> GetJobTitleAsync(string name, EntityId division, string token)
        {
            Logger.Debug("Getting Job Title [{0}]", name);

            var jobTitlesUri = $"/API/2.0/Data/Lookup/Employee/JobTitleId?select=Text&take=1&where=Text='{Uri.EscapeDataString(name)}'&token={token}";
            var resp = await Http.GetAsync(jobTitlesUri);
            if (!resp.IsSuccessStatusCode)
            {
                throw new DataException($"Error finding job title [{name}] from API:  ({resp.StatusCode}): {resp.ReasonPhrase}");
            }

            var content = await resp.Content.ReadAsStringAsync();
            var jobTitleResult = JObject.Parse(content);

            if (jobTitleResult["data"].HasValues)
            {
                return jobTitleResult["data"][0]["id"].Value<string>();
            }

            return null;
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
            var resp = await Http.GetAsync(orgHostUri);
            if (resp.IsSuccessStatusCode)
            {
                var hostAddressResp = new
                {
                    host = ""
                };

                var hostAddress = await GetResponseJson(resp, hostAddressResp);

                return hostAddress.host;
            }
            
            throw new DataException($"Cannot get organization host address at: {orgHostUri}");
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
            Http = new HttpClient();
            Http.BaseAddress = new Uri(baseAddress);
            Http.DefaultRequestHeaders.Accept.Clear();
            Http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // get the api host address based on the organization
            var hostAddress = await GetHostAddressAsync(organization);
            if (hostAddress != baseAddress)
            {
                Http.BaseAddress = new Uri(hostAddress);
            }

            // make the request to authenticate with the api
            var loginData = new
            {
                organization,
                username,
                password,
                culture = "en-US"
            };

            var loginPayload = JsonConvert.SerializeObject(loginData);
            var loginContent = new StringContent(loginPayload, Encoding.UTF8, "application/json");
            const string loginUri = "/API/2.0/Security/Login";

            string token = null;
            var resp = await Http.PostAsync(loginUri, loginContent);
            if (resp.IsSuccessStatusCode)
            {
                var loginResponse = new
                {
                    token = ""
                };

                var loginToken = await GetResponseJson(resp, loginResponse);
                token = loginToken.token;
            }
            else
            {
                throw new DataException($"Error logging in to organization. ({resp.StatusCode}): {resp.ReasonPhrase}");
            }

            return token;
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
            var resp = await Http.GetAsync(logoutUri);
            if (!resp.IsSuccessStatusCode)
            {
                throw new DataException($"Unable to logout user with token: [{token}].");
            }

            return true;
        }

        /// <summary>
        /// Updates an employee using the API
        /// </summary>
        /// <param name="employeeId">Id token of the employee to update</param>
        /// <param name="employee">The employee metadata object</param>
        /// <param name="token">The current active auth token</param>
        /// <returns>Void</returns>
        public async Task<bool> UpdateEmployeeAsync(EntityId employeeId, MetaObject employee, string token)
        {
            Logger.Debug("Updating Employee [{0}]", employeeId);

            var createEmployeeUri = $"/API/2.0/Data/EmployeeManagement/Employee/Default/{employeeId.Id}?token={token}";
            
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

            var employeeData = JsonConvert.SerializeObject(employee);
            var createEmployeeContent = new StringContent(employeeData, Encoding.UTF8, "application/json");

            var resp = await Http.PostAsync(createEmployeeUri, createEmployeeContent);
            if (!resp.IsSuccessStatusCode)
            {
                throw new DataException($"Error updating employee using API:  ({resp.StatusCode}): {resp.ReasonPhrase}\n{createEmployeeUri}\n{employeeData}");
            }

            var content = await resp.Content.ReadAsStringAsync();

            return true;
        }

        /// <summary>
        /// Updates an employee profile to add/remove group membership
        /// </summary>
        /// <param name="profileId">Id of the profile to update.</param>
        /// <param name="groupsToAdd">List of groups to add to the user.</param>
        /// <param name="groupsToRemove">List of groups that should be removed from the user.</param>
        /// <param name="token">Current auth token.</param>
        /// <returns></returns>
        public async Task<bool> UpdateEmployeeProfileAsync(EntityId profileId, List<EntityId> groupsToAdd, List<EntityId> groupsToRemove, string token)
        {
            Logger.Debug("Updating Employee Profile [{0}]", profileId.Token);

            if (groupsToAdd == null || groupsToAdd.Count == 0)
                return true;

            var updateProfileUri = $"/API/2.0/Data/EmployeeManagement/EmployeeProfile/Default/{profileId.Id}?token={token}";

            var groups = new List<Dictionary<string, string>>();

            groupsToAdd.ForEach(g =>
            {
                var grp = new Dictionary<string, string>
                {
                    {"id", g.Token},
                    {"action", "Add"}
                };
                groups.Add(grp);    
            });

            groupsToRemove.ForEach(g =>
            {
                var grp = new Dictionary<string, string>
                {
                    {"id", g.Token},
                    {"action", "Remove"}
                };
                groups.Add(grp);
            });

            var profile = new Dictionary<string, object>
            {
                {"Groups", groups}
            };

            var groupData = JsonConvert.SerializeObject(profile);
            var content = new StringContent(groupData, Encoding.UTF8, "application/json");

            var resp = await Http.PostAsync(updateProfileUri, content);
            if (!resp.IsSuccessStatusCode)
            {
                throw new DataException($"Error updating EmployeeProfile using API:  ({resp.StatusCode}): {resp.ReasonPhrase}\n{updateProfileUri}\n{groupData}");
            }

            var respContent = await resp.Content.ReadAsStringAsync();
            
            return true;
        }

        /// <summary>
        /// Reads the content from an http request and returns the json object
        /// </summary>
        /// <param name="response">The HTTP Response</param>
        /// <param name="anonymousType">The type of object to return</param>
        /// <returns>Deserialized response.</returns>
        private async Task<T> GetResponseJson<T>(HttpResponseMessage response, T anonymousType)
        {
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeAnonymousType(content, anonymousType);
            return json;
        }
    }
}
