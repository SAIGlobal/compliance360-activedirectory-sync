using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Compliance360.EmployeeSync.ApiV2Stream.Data;
using Compliance360.EmployeeSync.ApiV2Stream.Services;
using Compliance360.EmployeeSync.Library.Configuration;
using Compliance360.EmployeeSync.Library.Data;
using Compliance360.EmployeeSync.Library.OutputStreams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Compliance360.EmployeeSync.ApiV2Stream
{
    public class ApiStream : IOutputStream
    {
        private static class SystemFields
        {
            public const string Department = "Department";
            public const string Groups = "Groups";
            public const string JobTitleId = "JobTitleId";
            public const string PrimaryDivision = "PrimaryDivision";
            public const string WorkflowTemplate = "WorkflowTemplate";
            public const string EmployeeNum = "EmployeeNum";
            public const string Password = "Password";
            public const string CanLogin = "CanLogin";
            public const string Relationships = "Relationships";
        }

        private class LoginSettings
        {
            public string BaseAddress;
            public string Organization;
            public string Username;
            public string Password;
        }

        private readonly ConcurrentDictionary<string, string> _tokenDict = new ConcurrentDictionary<string, string>();
        private ILogger Logger { get; }
        private JobElement JobConfig { get; set; }
        private StreamElement StreamConfig { get; set; }
        private string AuthToken {
            get { return _tokenDict["authToken"]; }
            set { _tokenDict["authToken"] = value; } 
        }
        private ICacheServiceFactory CacheServiceFactory { get; }
        private ICacheService EmployeeCache { get; set; }
        private ICacheService DivisionCache { get; set; }
        private ICacheService JobTitleCache { get; set; }
        private ICacheService DepartmentCache { get; set; }
        private ICacheService WorkflowCache { get; set; }
        private ICacheService EmployeeGroupCache { get; set; }
        private ICacheService EmployeeProfileCache { get; set; }
        private ICacheService GroupMembershipCache { get; set; }
        private ICacheService RelationshipTypeCache { get; set; }
        private ICacheService EmployeeDistinguishedNameCache { get; set; }
        private IAuthenticationService AuthenticationService { get; }
        private IDepartmentService DepartmentService { get; }
        private IDivisionService DivisionService { get; }
        private IEmployeeService EmployeeService { get; }
        private IGroupService GroupService { get; }
        private IRelationshipService RelationshipService { get; }

        private System.Threading.Timer _timer;
        private readonly Dictionary<Employee, SortedList<string, string>> _employeesWithRelationships = new Dictionary<Employee, SortedList<string, string>>();

        public ApiStream(
            ILogger logger,
            ICacheServiceFactory cacheServiceFactory,
            IAuthenticationService authenticationService,
            IDepartmentService departmentService,
            IDivisionService divisionService,
            IEmployeeService employeeService,
            IGroupService groupService,
            IRelationshipService relationshipService)
        {
            Logger = logger;
            CacheServiceFactory = cacheServiceFactory;
            AuthenticationService = authenticationService;
            DepartmentService = departmentService;
            DivisionService = divisionService;
            EmployeeService = employeeService;
            GroupService = groupService;
            RelationshipService = relationshipService;
        }

        public void Close()
        {
            // cancel the renew login process
            _timer?.Dispose();
            
            // close the cache services
            EmployeeCache?.WriteCacheEntries();
            JobTitleCache?.WriteCacheEntries();
            DepartmentCache?.WriteCacheEntries();
            WorkflowCache?.WriteCacheEntries();
            EmployeeGroupCache?.WriteCacheEntries();
            EmployeeProfileCache?.WriteCacheEntries();
            GroupMembershipCache?.WriteCacheEntries();
            RelationshipTypeCache?.WriteCacheEntries();
            EmployeeDistinguishedNameCache?.WriteCacheEntries();

            Logger.Debug("Closed the Api Stream");
        }

        public void Open(JobElement jobConfig, StreamElement streamConfig)
        {
            JobConfig = jobConfig;
            StreamConfig = streamConfig;

            // get the login interval
            int loginIntervalMinutes;
            if (!int.TryParse(streamConfig.Settings["loginIntervalMinutes"], out loginIntervalMinutes))
            {
                loginIntervalMinutes = 20;
            }
            var loginInterval = loginIntervalMinutes * 60 * 1000;

            // authenticate with the c360 api
            var loginSettings = new LoginSettings
            {
                BaseAddress = StreamConfig.Settings["baseAddress"],
                Organization = StreamConfig.Settings["organization"],
                Username = StreamConfig.Settings["username"],
                Password = StreamConfig.Settings["password"]
            };
            
            // get the base address
            var apiAddress =
                AuthenticationService.GetHostAddress(loginSettings.BaseAddress, loginSettings.Organization);
            loginSettings.BaseAddress = apiAddress;

            // set the base address for the api services
            DepartmentService.SetBaseAddress(apiAddress);
            DivisionService.SetBaseAddress(apiAddress);
            EmployeeService.SetBaseAddress(apiAddress);
            GroupService.SetBaseAddress(apiAddress);
            RelationshipService.SetBaseAddress(apiAddress);

            // setup a task to renew the token on a regular interval
            // authenticate with the c360 api
            _timer = new Timer(Login, loginSettings, loginInterval, loginInterval);
            
            // load the cache services for cached content
            EmployeeCache = CacheServiceFactory.CreateCacheService(Logger, "Employee", false);
            JobTitleCache = CacheServiceFactory.CreateCacheService(Logger, "JobTitle", false);
            DepartmentCache = CacheServiceFactory.CreateCacheService(Logger, "Department", false);
            WorkflowCache = CacheServiceFactory.CreateCacheService(Logger, "Workflow", false);
            EmployeeGroupCache = CacheServiceFactory.CreateCacheService(Logger, "EmployeeGroup", true);
            EmployeeProfileCache = CacheServiceFactory.CreateCacheService(Logger, "EmployeeProfile", false);
            DivisionCache = CacheServiceFactory.CreateCacheService(Logger, "Division", false);
            GroupMembershipCache = CacheServiceFactory.CreateCacheService(Logger, "GroupMembership", false);
            RelationshipTypeCache = CacheServiceFactory.CreateCacheService(Logger, "RelationshipType", false);
            EmployeeDistinguishedNameCache =
                CacheServiceFactory.CreateCacheService(Logger, "EmployeeDistinguishedName", true);
        }

        /// <summary>
        /// Logs into the C360 API as the configured user.
        /// </summary>
        /// <param name="state"></param>
        private void Login(object state)
        {
            try
            {
                var loginSettings = (LoginSettings) state;
                
                Logger.Debug("Logging in to API with BaseAddress:{0} Organization:{1} Username:{2} Password:{3}",
                    loginSettings.BaseAddress,
                    loginSettings.Organization,
                    loginSettings.Username,
                    loginSettings.Password);

                AuthToken = AuthenticationService.Login(loginSettings.BaseAddress,
                    loginSettings.Organization,
                    loginSettings.Username,
                    loginSettings.Password);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.ToString()}");
            }
            finally
            {
                
            }
        }

        /// <summary>
        /// Writes the user to the stream
        /// </summary>
        /// <param name="user">Reference to the user to write to the stream.</param>
        public void Write(ActiveDirectoryUser user)
        {
            var employee = new Employee();
            
            // process each of the field values in the map
            string employeeNum = null;
            Entity division = null;
            SortedList<string, string> groups = null;
            var relationships = new SortedList<string, string>();

            foreach (MapElement map in StreamConfig.Mapping)
            {
                object value = null;
                switch (map.To)
                {
                    case SystemFields.JobTitleId:
                        value = GetJobTitleValue(map.From, division, user);
                        break;

                    case SystemFields.EmployeeNum:
                        employeeNum = GetFieldValue(map.From, user);
                        value = employeeNum;
                        break;
                        
                    case SystemFields.Department:
                        value = GetDepartment(map.From, user, division);
                        break;

                    case SystemFields.PrimaryDivision:
                        division = GetDivisionFieldValue(map.From, user);
                        value = division;
                        break;

                    case SystemFields.Groups:
                        groups = GetFieldValueAsObject(map.From, user) as SortedList<string, string>;
                        break;

                    case SystemFields.WorkflowTemplate:
                        break;

                    case SystemFields.Relationships:
                        relationships[map.Type] = GetFieldValue(map.From, user);
                        break;
                        
                    default:
                        value = GetFieldValue(map.From, user);
                        break;
                }

                // only map the property if a value is present
                if (value != null)
                {
                    employee[map.To] = value;
                }
            }

            ProcessUserData(user,
                employee,
                employeeNum,
                division,
                groups,
                relationships);
        }

        public void ProcessUserData(ActiveDirectoryUser user, 
            Employee employee,
            string employeeNum,
            Entity division,
            SortedList<string, string> groups,
            SortedList<string, string> relationships)
        {
            if (employeeNum == null)
            {
                var userJson = JsonConvert.SerializeObject(user);
                Logger.Error("Error writing Employee to stream. Missing EmployeeNum map \"to\" value. {0}", userJson);
                return;
            }

            if (division == null)
            {
                var userJson = JsonConvert.SerializeObject(user);
                Logger.Error("Error writing Employee to stream. Missing PrimaryDivision map \"to\" value. {0}", userJson);
                return;
            }
            
            // get the id of the employee from the cache...if found this is an update
            // otherwise we are creating a new employee.
            if (EmployeeCache.ContainsKey(employeeNum))
            {
                employee.Id = EmployeeCache.GetValue(employeeNum);
            }
            else
            {
                // check to see if the username already exists in c360
                var empId = ApiService.GetEmployeeIdAsync(employeeNum, AuthToken).GetAwaiter().GetResult();
                if (empId != null)
                {
                    employee.Id = empId;
                    EmployeeCache.Add(employeeNum, employee.Id);
                }
            }

            if (employee.Id == null)
            {
                // create the new employee record
                var workflowTemplate = GetDefaultWorkflowTemplate();
                employee[SystemFields.WorkflowTemplate] = workflowTemplate;
                
                employee[SystemFields.CanLogin] = true;
                employee[SystemFields.Password] = Guid.NewGuid().ToString();

                var newEmployeeId = ApiService.CreateEmployeeAsync(employee, AuthToken).GetAwaiter().GetResult();
                employee.Id = newEmployeeId;
                EmployeeCache.Add(employeeNum, newEmployeeId);
            }
            else
            {
                // update an existing employee record
                ApiService.UpdateEmployeeAsync(employee, AuthToken).GetAwaiter().GetResult();
            }

            // ensure the employee is in the DN cache
            var dnCacheKey = user.Attributes["distinguishedName"].ToString();
            EmployeeDistinguishedNameCache.Add(dnCacheKey, employee.Id);

            if (groups != null)
            {
                ProcessGroupChanges(employee, groups.Values.ToList());
            }

            if (relationships != null)
            {
                // this employee has relationships, store them
                // for future processing once all of the employees have been created
                _employeesWithRelationships[employee] = relationships;
            }
        }

        /// <summary>
        /// Saves the relationships from AD to the C360 application
        /// </summary>
        /// <param name="employee">The employee that owns the relationships</param>
        /// <param name="relationships">The relatiobships to add/update</param>
        public void ProcessRelationshipChanges(Employee employee, SortedList<string, string> relationships)
        {
            // fetch the online relationship info
            var onlineRelationships = ApiService.GetEmployeeRelationships(employee, AuthToken).Result;
            var onlineRelationDetails = new List<GetEmployeeRelationshipDetailsResponse>();
            foreach (var relationshipId in onlineRelationships)
            {
                var relationDetails =
                    ApiService.GetEmployeeRelationshipDetails(new Entity {Id = relationshipId}, AuthToken).Result;
                onlineRelationDetails.Add(relationDetails);
            }
            
            // process each of the relationships
            foreach (var relTypeName in relationships.Keys)
            {
                // get the id of the employee...must be in the cache
                var dn = relationships[relTypeName];
                if (!EmployeeDistinguishedNameCache.ContainsKey(dn))
                {
                    Logger.Info($"Did not find employee for DN [{dn}] in the cache. Relationship will not be added.");
                    continue;
                }

                var destEmployeeId = EmployeeDistinguishedNameCache.GetValue(dn);

                // get the id of the correct relationship type
                Entity relType = null;
                if (RelationshipTypeCache.ContainsKey(relTypeName))
                {
                    relType = new Entity {Id = RelationshipTypeCache.GetValue(relTypeName)};
                }
                else
                {
                    // try to look up the type
                    var typeId = ApiService.GetRelationshipTypeByNameAsync(relTypeName, AuthToken).Result;
                    if (!string.IsNullOrEmpty(typeId))
                    {
                        // found it put it in the cache
                        relType = new Entity {Id = typeId};
                        RelationshipTypeCache.Add(relTypeName, relType.Id);
                    }
                    else
                    {
                        // was not present...need to create it
                        var id = ApiService.CreateRelationshipTypeAsync(relTypeName, AuthToken).Result;
                        relType = new Entity { Id = id };
                        RelationshipTypeCache.Add(relTypeName, relType.Id);
                    }
                }
                
                // see if we can find the relationship in the online list
                bool processed = false;
                foreach (var onlineRel in onlineRelationDetails)
                {
                    if (onlineRel.Type.Id == relType.Id && onlineRel.Employee.Id == destEmployeeId)
                    {
                        // relationship exists..do nothing
                        processed = true;
                        break;
                    }
                    else if (onlineRel.Type.Id == relType.Id)
                    {
                        // if it is not the same, update it
                        var res = ApiService.UpdateRelationshipAsync(onlineRel, destEmployeeId, AuthToken).Result;
                        processed = true;
                        break;
                    }
                }
                
                // not found create it
                if (!processed)
                {
                    var newRelId = ApiService.CreateRelationshipAsync(employee, destEmployeeId, relType, AuthToken).Result;
                }
            }
        }

        /// <summary>
        /// Returns the default workflow template
        /// </summary>
        /// <returns></returns>
        public Entity GetDefaultWorkflowTemplate()
        {
            const string DefaultTemplateName = "SystemDefault";

            // check the cache
            if (WorkflowCache.ContainsKey(DefaultTemplateName))
            {
                return new Entity { Id = WorkflowCache.GetValue(DefaultTemplateName) };
            }

            // workflow does not exist in the cache. Get it from the service
            var workflowTemplateId = ApiService.GetDefaultWorkflowTemplateAsync(AuthToken).GetAwaiter().GetResult();
            if (workflowTemplateId != null)
            {
                WorkflowCache.Add(DefaultTemplateName, workflowTemplateId);
                return new Entity { Id = workflowTemplateId };
            }

            return null;
        }

        /// <summary>
        /// Gets a department id based on its name.
        /// </summary>
        /// <param name="from">The from value to parse</param>
        /// <param name="user">The user object that contains the data.</param>
        /// <param name="division"></param>
        /// <returns></returns>
        public Entity GetDepartment(string from, 
            ActiveDirectoryUser user, 
            Entity division)
        {
            // get the department name
            var departmentName = GetFieldValue(from, user);
            if (string.IsNullOrEmpty(departmentName))
            {
                return null;
            }

            // check the cache
            if (DepartmentCache.ContainsKey(departmentName))
            {
                return new Entity { Id = DepartmentCache.GetValue(departmentName) };
            }

            // did not find the department in the cache
            // try to get it from the serice
            var departmentId = ApiService.GetDepartmentAsync(departmentName, division, AuthToken).GetAwaiter().GetResult();
            if (departmentId != null)
            {
                DepartmentCache.Add(departmentName, departmentId);
                return new Entity { Id = departmentId };
            }

            // we did not find the department in the service...create it
            departmentId = ApiService.CreateDepartmentAsync(departmentName, division, AuthToken).GetAwaiter().GetResult();
            if (departmentId != null)
            {
                DepartmentCache.Add(departmentName, departmentId);
                return new Entity { Id = departmentId };
            }

            return null;
        }

        /// <summary>
        /// Gets the division id value
        /// </summary>
        /// <param name="from">The from value to parse</param>
        /// <param name="user">The user object that contains the data.</param>
        /// <returns></returns>
        public Entity GetDivisionFieldValue(string from, ActiveDirectoryUser user)
        {
            // get the division name value
            var divisionName = GetFieldValue(from, user);

            // check the cache
            if (DivisionCache.ContainsKey(divisionName))
            {
                return new Entity { Id = DivisionCache.GetValue(divisionName) };
            }

            // division does not exist in the cache...get it from the service
            var divisionId = ApiService.GetDivisionAsync(divisionName, AuthToken).GetAwaiter().GetResult();
            if (divisionId != null)
            {
                DivisionCache.Add(divisionName, divisionId);
                return new Entity { Id = divisionId };
            }

            return null;
        }

        /// <summary>
        /// Gets the value from the user object
        /// </summary>
        /// <param name="from">The from value to parse</param>
        /// <param name="user">The user object that contains the data.</param>
        /// <returns></returns>
        public string GetFieldValue(string from, ActiveDirectoryUser user)
        {
            var fieldValue = from;
            var regex = new Regex("{[^}]*}");
            var matches = regex.Matches(from);
            foreach (Match match in matches)
            {
                try
                {
                    var key = match.Value.Substring(1, match.Value.Length - 2);
                    if (user.Attributes.ContainsKey(key) && !string.IsNullOrEmpty(match.Value))
                    {
                        var val = user.Attributes[key] ?? string.Empty;
                        fieldValue = fieldValue.Replace(match.Value, val.ToString());
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }

            return fieldValue;
        }

        /// <summary>
        /// Gets the value from the user object
        /// </summary>
        /// <param name="from">The from value to parse</param>
        /// <param name="user">The user object that contains the data.</param>
        /// <returns></returns>
        public object GetFieldValueAsObject(string from, ActiveDirectoryUser user)
        {
            var fieldValue = from;
            var regex = new Regex("{[^}]*}");
            var matches = regex.Matches(from);
            foreach (Match match in matches)
            {
                var key = match.Value.Substring(1, match.Value.Length - 2);
                if (user.Attributes.ContainsKey(key))
                {
                    return user.Attributes[key];
                }
            }

            return fieldValue;
        }
        
        /// <summary>
        /// Gets a JobTitle value 
        /// </summary>
        /// <param name="from">The from value to parse.</param>
        /// <param name="division">The id of the division.</param>
        /// <param name="user">The user object that contains the data.</param>
        /// <returns></returns>
        public Entity GetJobTitleValue(string from, Entity division, ActiveDirectoryUser user)
        {
            // get the field value for the job title name
            var jobTitleValue = GetFieldValue(from, user);
            if (string.IsNullOrEmpty(jobTitleValue))
            {
                return null;
            }

            // check the cache 
            if (JobTitleCache.ContainsKey(jobTitleValue))
                return new Entity { Id = JobTitleCache.GetValue(jobTitleValue) };

            // job title does not exist in the cache...get it from the service
            var jobTitleId = ApiService.GetJobTitleAsync(jobTitleValue, division, AuthToken).GetAwaiter().GetResult();
            if (jobTitleId != null)
            {
                JobTitleCache.Add(jobTitleValue, jobTitleId);
                return new Entity { Id = jobTitleId };
            }

            // did not find the job title in the system so add it.
            jobTitleId = ApiService.CreateJobTitleAsync(jobTitleValue, division, AuthToken).GetAwaiter().GetResult();
            if (jobTitleId != null)
            {
                JobTitleCache.Add(jobTitleValue, jobTitleId);
                return new Entity { Id = jobTitleId };
            }

            return null;
        }

        /// <summary>
        /// Processes the groups from AD and updates the users as needed.
        /// </summary>
        /// <param name="employee">The id of the employee</param>
        /// <param name="groupNames">The names of the groups the user is a member of</param>
        public void ProcessGroupChanges(Employee employee, List<string> groupNames)
        {
            // get the id of the user's profile
            Profile profile = null;
            if (EmployeeProfileCache.ContainsKey(employee.Id))
            {
                profile = new Profile { Id = EmployeeProfileCache.GetValue(employee.Id) };
            }

            if (profile == null)
            {
                // did not find it in the cache...get the id from the api
                var employeeProfileId = ApiService.GetEmployeeProfileIdAsync(new Entity{ Id = employee.Id }, AuthToken).GetAwaiter().GetResult();
                profile = new Profile { Id = employeeProfileId };
            }

            // get the existing cached group membership
            var cachedGroupsIds = new HashSet<string>();
            if (GroupMembershipCache.ContainsKey(employee.Id))
            {
                var ids = GroupMembershipCache.GetValue(employee.Id).Split(',');
                ids.ToList().ForEach(id =>
                {
                    if (!cachedGroupsIds.Contains(id))
                    {
                        cachedGroupsIds.Add(id);
                    }
                });
            }
           
            var onlineGroupsIds = ApiService.GetGroupMembershipAsync(profile, AuthToken).GetAwaiter().GetResult();

            // we only get Ids back from the API so we need to further process them
            // so that we xref the Ids to the group names returned by AD
            var onlineGroups = ProcessOnlineGroups(onlineGroupsIds);

            // if not then add them to the user
            var groupsToAdd = new List<Entity>();
            var adGroupIds = new List<string>();
            groupNames.ForEach(grpName =>
            {
                var groupId = EmployeeGroupCache.GetValue(grpName);
                if (groupId == null)
                {
                    // try to get the group by its name using the api
                    groupId = ApiService.GetGroupAsync(grpName, AuthToken).GetAwaiter().GetResult();
                }

                if (groupId == null)
                { 
                    // group is not in the system and needs to be created.
                    groupId = ApiService.CreateGroupAsync(grpName, AuthToken).GetAwaiter().GetResult();
                    EmployeeGroupCache.Add(groupId, grpName);
                }

                // add the group to the list of AD groups...we will use this list to
                // figure out which groups need to be removed from Compliance 360
                adGroupIds.Add(groupId);
                
                // if the user is not a current member of the group then
                // put it in the list to add to the profile
                if (!onlineGroups.ContainsKey(groupId))
                {
                    groupsToAdd.Add(new Entity { Id = groupId } );

                    cachedGroupsIds.Add(groupId);
                }
            });

            // check the list of online group Ids. If they are not in the list
            // of local AD groups and we have record that we added the user to the group them remove it
            var groupsToRemove = new List<Entity>();
            foreach (var onlineGroupId in onlineGroupsIds)
            {
                // bit of a hack but we do not want to remove the user from the 
                // division employees groups
                if (onlineGroups[onlineGroupId].Contains("Employees"))
                    continue;

                if (!adGroupIds.Contains(onlineGroupId) && cachedGroupsIds.Contains(onlineGroupId))
                {
                    groupsToRemove.Add(new Entity { Id = onlineGroupId });
                    cachedGroupsIds.Remove(onlineGroupId);
                }
            }

            ApiService.UpdateEmployeeProfileAsync(profile, groupsToAdd, groupsToRemove, AuthToken);

            GroupMembershipCache.Add(employee.Id, String.Join(",", cachedGroupsIds));
        }

        /// <summary>
        /// Process the list of group ids and ensure their names are in the 
        /// cache
        /// </summary>
        /// <param name="onlineGroups"></param>
        /// <returns></returns>
        public Dictionary<string, string> ProcessOnlineGroups(List<string> onlineGroupIds)
        {
            var userGroups = new Dictionary<string, string>();

            // check cache for item
            foreach(var grpId in onlineGroupIds)
            { 
                if (EmployeeGroupCache.ContainsKey(grpId))
                {
                    userGroups[grpId] = EmployeeGroupCache.GetValue(grpId);
                }
                else
                {
                    // did not find it in cache...get name from api
                    var groupName = ApiService.GetGroupNameAsync(grpId, AuthToken).GetAwaiter().GetResult();

                    // add it to the cache so that we do not have to 
                    // look it up again
                    EmployeeGroupCache.Add(grpId, groupName);

                    userGroups[grpId] = groupName;
                }
            }

            return userGroups;
        }

        public void StreamComplete()
        {
            if (_employeesWithRelationships != null && _employeesWithRelationships.Count > 0)
            {
                foreach (var employee in _employeesWithRelationships.Keys)
                {
                    ProcessRelationshipChanges(employee, _employeesWithRelationships[employee]);
                }
            }
        }
    }
}
