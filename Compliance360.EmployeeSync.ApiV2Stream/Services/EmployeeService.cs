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

        public async Task<Entity> CreateJobTitleAsync(string jobTitleName, string token)
        {
            Logger.Debug("Creating Job Title [{0}]", jobTitleName);

            var createJobTitleUri = $"/API/2.0/Data/Lookup/Employee/JobTitleId?token={token}";

            var jobTitle = new Dictionary<string, object>
            {
                { "Text", jobTitleName }
            };

            var result = await Http.PostAsync<CreateResponse>(createJobTitleUri, jobTitle);

            return new Entity {Id = result.Id};
        }

        public Entity CreateJobTitle(string jobTitleName, string token)
        {
            return CreateJobTitleAsync(jobTitleName, token).Result;
        }

        public async Task<Entity> GetDefaultWorkflowTemplateAsync(string token)
        {
            Logger.Debug("Getting default Workflow");

            var getWorkflowTemplateUri =
                $"/API/2.0/Data/Global/WorkflowTemplates/Employee?take=1&where=IsDefault='True'&token={token}";

            var resp = await Http.GetAsync<GetResponse<Entity>>(getWorkflowTemplateUri);

            var id = resp.Data?.FirstOrDefault()?.Id;
            return id != null ? new Entity {Id = id} : null;
        }

        public Entity GetDefaultWorkflowTemplate(string token)
        {
            return GetDefaultWorkflowTemplateAsync(token).Result;
        }

        public async Task<Entity> GetEmployeeByEmployeeNumAsync(string employeeNum, string token)
        {
            Logger.Debug("Getting Id of Employee using EmployeeNumber [{0}]", employeeNum);

            var findEmployeeUri = $"/API/2.0/Data/EmployeeManagement/Employee/Default?take=1&where=EmployeeNum='{Uri.EscapeDataString(employeeNum)}'&token={token}";

            var resp = await Http.GetAsync<GetResponse<Entity>>(findEmployeeUri);

            var id = resp.Data?.FirstOrDefault()?.Id;
            return id != null ? new Entity {Id = id} : null;
        }

        public Entity GetEmployeeByEmployeeNum(string employeeNum, string token)
        {
            return GetEmployeeByEmployeeNumAsync(employeeNum, token).Result;
        }

        public async Task<Entity> GetEmployeeProfileAsync(Entity employee, string token)
        {
            Logger.Debug("Getting Profile for Employee [{0}]", employee.Id);

            var getEmployeeProfileUri = $"/API/2.0/Data/EmployeeManagement/Employee/Default?select=Profile&where=InstanceId='{employee.Id}'&token={token}";

            var resp = await Http.GetAsync<GetResponse<GetEmployeeProfileIdResponse>>(getEmployeeProfileUri);

            var id = resp.Data?.FirstOrDefault()?.Profile.Id;
            return id != null ? new Entity { Id = id } : null;
        }

        public Entity GetEmployeeProfile(Entity employee, string token)
        {
            return GetEmployeeProfileAsync(employee, token).Result;
        }

        public async Task<Entity> GetJobTitleAsync(string name, string token)
        {
            Logger.Debug("Getting Job Title [{0}]", name);

            var jobTitlesUri = $"/API/2.0/Data/Lookup/Employee/JobTitleId?select=Text&take=1&where=Text='{Uri.EscapeDataString(name)}'&token={token}";
            var resp = await Http.GetAsync<GetResponse<Entity>>(jobTitlesUri);

            var id = resp.Data?.FirstOrDefault()?.Id;
            return id != null ? new Entity { Id = id } : null;
        }

        public Entity GetJobTitle(string name, string token)
        {
            return GetJobTitleAsync(name, token).Result;
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

        public void UpdateEmployee(Employee employee, string token)
        {
            UpdateEmployeeAsync(employee, token).GetAwaiter().GetResult();
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

            var updateProFileRequest = new UpdateProfile {Groups = profile.Groups};

            await Http.PostAsync<CreateResponse>(updateProfileUri, updateProFileRequest);

            return true;
        }

        public void UpdateEmployeeProfile(Profile profile,
            List<Entity> groupsToAdd,
            List<Entity> groupsToRemove,
            string token)
        {
            UpdateEmployeeProfileAsync(profile, groupsToAdd, groupsToRemove, token).GetAwaiter().GetResult();
        }
    }
}
