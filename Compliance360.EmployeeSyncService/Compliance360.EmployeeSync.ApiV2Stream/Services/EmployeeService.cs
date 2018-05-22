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

namespace Compliance360.EmployeeSync.ApiV2Stream.Services
{
    public class EmployeeService : IEmployeeService
    {
        private ILogger Logger { get; }
        private IHttpDataService Http { get; }

        public EmployeeService(ILogger logger, IHttpDataService http)
        {
            Logger = logger;
            Http = http;
        }

        public void SetBaseAddress(string baseAddress)
        {
            Http.Initialize(baseAddress);
        }
        
        public async Task<Entity> CreateEmployeeAsync(Employee employee, string token)
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

            return new Entity {Id = result.Id};
        }

        public Entity CreateEmployee(Employee employee, string token)
        {
            return CreateEmployeeAsync(employee, token).Result;
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

            var createJobTitleUri = $"/API/2.0/Data/Lookup/Employee/JobTitleId?token={token}";

            var jobTitle = new Dictionary<string, object>
            {
                { "Text", jobTitleName }
            };

            var result = await Http.PostAsync<CreateResponse>(createJobTitleUri, jobTitle);

            return result.Id;
        }

        
        public async Task<string> CreateRelationshipTypeAsync(string relationshipType, string token)
        {
            Logger.Debug("Creating Relationship Type [{0}]", relationshipType);

            var createRelationshipTypeUri = $"/API/2.0/Data/Lookup/EmployeeRelationship/Type?token={token}";

            var typeToCreate = new Dictionary<string, object>
            {
                { "Text", relationshipType }
            };

            var result = await Http.PostAsync<CreateResponse>(createRelationshipTypeUri, typeToCreate);

            return result.Id;
        }

        public async Task<string> CreateRelationshipAsync(Employee employee, string destEmployeeId, Entity relType, string token)
        {
            Logger.Debug("Creating Relationship for Employee [{0}], related to [{1}]", employee.Id, destEmployeeId);

            var createRelationshipUri = $"/API/2.0/Data/EmployeeManagement/EmployeeRelationship/Default?token={token}";

            var relationshipToCreate = new Dictionary<string, object>
            {
                { "Employee", new Entity { Id = destEmployeeId } },
                { "Type", relType }
            };

            var newRelationship = await Http.PostAsync<CreateResponse>(createRelationshipUri, relationshipToCreate);

            return newRelationship.Id;
        }

        
        public async Task<string> GetDefaultWorkflowTemplateAsync(string token)
        {
            Logger.Debug("Getting default Workflow");

            var getWorkflowTemplateUri =
                $"/API/2.0/Data/Global/WorkflowTemplates/Employee?take=1&where=IsDefault='True'&token={token}";

            var resp = await Http.GetAsync<GetResponse<Entity>>(getWorkflowTemplateUri);

            return resp.Data?.FirstOrDefault()?.Id;
        }

        
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

        
        public async Task<string> GetDivisionAsync(string divisionPath, string token)
        {
            Logger.Debug("Getting division [{0}]", divisionPath);

            var findDivisionUri =
                $"/API/2.0/Data/EmployeeManagement/EmployeeDivision/Default?take=1&where=Path='{Uri.EscapeDataString(divisionPath)}'&token={token}";

            var resp = await Http.GetAsync<GetResponse<Entity>>(findDivisionUri);

            return resp.Data?.FirstOrDefault()?.Id;
        }

        
        public async Task<string> GetEmployeeIdAsync(string employeeNum, string token)
        {
            Logger.Debug("Getting Id of Employee using EmployeeNumber [{0}]", employeeNum);

            var findEmployeeUri = $"/API/2.0/Data/EmployeeManagement/Employee/Default?take=1&where=EmployeeNum='{Uri.EscapeDataString(employeeNum)}'&token={token}";

            var resp = await Http.GetAsync<GetResponse<Entity>>(findEmployeeUri);

            return resp.Data?.FirstOrDefault()?.Id;
        }

        
        public async Task<string> GetEmployeeProfileIdAsync(Entity employee, string token)
        {
            Logger.Debug("Getting Profile for Employee [{0}]", employee.Id);

            var getEmployeeProfileUri = $"/API/2.0/Data/EmployeeManagement/Employee/Default?select=Profile&where=InstanceId='{employee.Id}'&token={token}";

            var resp = await Http.GetAsync<GetResponse<GetEmployeeProfileIdResponse>>(getEmployeeProfileUri);

            return resp.Data?.FirstOrDefault()?.Profile.Id;
        }

        
        public async Task<List<string>> GetEmployeeRelationships(Employee employee, string token)
        {
            Logger.Debug("Getting Relationships for Employee [{0}]", employee.Id);

            var getEmployeeRelationshipsUri = $"/API/2.0/Data/EmployeeManagement/Employee/Default?select=RelatedEmployees&where=InstanceId='{employee.Id}'&token={token}";

            var resp = await Http.GetAsync<GetResponse<GetEmployeeRelationshipResponse>>(getEmployeeRelationshipsUri);

            var ids = resp.Data?.FirstOrDefault()?.RelatedEmployees.Select(re => re.Id);

            return ids?.ToList();
        }

        public async Task<Relationship> GetEmployeeRelationshipDetails(Entity relationship, string token)
        {
            Logger.Debug("Getting details for Relationship [{0}]", relationship.Id);

            var getEmployeeRelationshipUri = $"/API/2.0/Data/EmployeeManagement/EmployeeRelationship/Default?select=Employee,Type&where=InstanceId='{relationship.Id}'&token={token}";

            var resp = await Http.GetAsync<GetResponse<Relationship>>(getEmployeeRelationshipUri);

            return resp.Data?.FirstOrDefault();
        }

        
        public async Task<string> GetGroupAsync(string groupName, string token)
        {
            Logger.Debug("Getting Group [{0}]", groupName);

            var getGroupUri =
                $"/API/2.0/Data/EmployeeManagement/EmployeeGroup/Default?where=GroupName='{Uri.EscapeDataString(groupName)}'&take=1&token={token}";

            var resp = await Http.GetAsync<GetResponse<Entity>>(getGroupUri);

            return resp.Data?.FirstOrDefault()?.Id;
        }

        
        public async Task<string> GetGroupsFolderAsync(string token)
        {
            Logger.Debug("Getting \"Groups\" root folder");

            var getGroupsFolderUri =
                $"/API/2.0/Data/Global/Folders/Default?select=Name,Parent,Division&where=Name='Groups'&token={token}";

            var resp = await Http.GetAsync<GetResponse<Folder>>(getGroupsFolderUri);

            var folder = resp.Data?.FirstOrDefault(f => f.Parent.Id == "NULL" && f.Division.Id == "NULL");
            return folder?.Id;
        }

        
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

        
        public async Task<string> GetGroupNameAsync(string groupId, string token)
        {
            Logger.Debug("Getting Group Name for [{0}]", groupId);

            var getGroupUri =
                $"/API/2.0/Data/EmployeeManagement/EmployeeGroup/Default?select=GroupName&where=InstanceId='{groupId}'&take=1&token={token}";

            var resp = await Http.GetAsync<GetResponse<EmployeeGroup>>(getGroupUri);

            return resp.Data?.FirstOrDefault()?.GroupName;
        }   

        public async Task<string> GetJobTitleAsync(string name, Entity division, string token)
        {
            Logger.Debug("Getting Job Title [{0}]", name);

            var jobTitlesUri = $"/API/2.0/Data/Lookup/Employee/JobTitleId?select=Text&take=1&where=Text='{Uri.EscapeDataString(name)}'&token={token}";
            var resp = await Http.GetAsync<GetResponse<Entity>>(jobTitlesUri);

            return resp.Data?.FirstOrDefault()?.Id;
        }
        
        public async Task<string> GetRelationshipTypeByNameAsync(string name, string token)
        {
            Logger.Debug("Getting Relationship Type [{0}]", name);

            var jobTitlesUri = $"/API/2.0/Data/Lookup/EmployeeRelationship/Type?select=Text&take=1&where=Text='{Uri.EscapeDataString(name)}'&token={token}";
            var resp = await Http.GetAsync<GetResponse<Entity>>(jobTitlesUri);

            return resp.Data?.FirstOrDefault()?.Id;
        }

       
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

        
        public async Task<bool> LogoutAsync(string token)
        {
            Logger.Debug("Logging out of API");

            var logoutUri = $"/API/2.0/Security/Logout?token={Uri.EscapeUriString(token)}";
            await Http.GetAsync(logoutUri);

            return true;
        }

        
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

        public async Task<bool> UpdateEmployeeProfileAsync(Profile profile, List<Entity> groupsToAdd, List<Entity> groupsToRemove, string token)
        {
            Logger.Debug("Updating Employee Profile [{0}]", profile.Id);

            if (groupsToAdd == null || groupsToAdd.Count == 0)
                return true;

            var updateProfileUri = $"/API/2.0/Data/EmployeeManagement/EmployeeProfile/Default/{profile.InstanceId}?token={token}";

            profile.Groups = new List<EntityReference>();

            groupsToAdd.ForEach(g =>
            {
                profile.Groups.Add(new EntityReference { Action = "Add", Id = g.Id });
            });

            groupsToRemove.ForEach(g =>
            {
                profile.Groups.Add(new EntityReference { Action = "Remove", Id = g.Id });
            });

            await Http.PostAsync<CreateResponse>(updateProfileUri, profile);

            return true;
        }

        public async Task<bool> UpdateRelationshipAsync(Relationship relationship,
            string destEmployeeId,
            string token)
        {
            Logger.Debug("Updating Employee Relationship [{0}]", relationship.Id);

            var updateRelationshipUri = $"/API/2.0/Data/EmployeeManagement/EmployeeRelationship/Default/{relationship.InstanceId}?token={token}";

            var relToUpdate = new Relationship
            {
                Employee = new Entity { Id = destEmployeeId },
                Type = relationship.Type
            };

            await Http.PostAsync<CreateResponse>(updateRelationshipUri, relToUpdate);

            return true;
        }
    }
}
