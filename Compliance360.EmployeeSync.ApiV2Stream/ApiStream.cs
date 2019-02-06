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
            //public const string JobTitleId = "JobTitleId";
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
        private ICacheService LookupCache { get; set; }
        private IAuthenticationService AuthenticationService { get; }
        private IDepartmentService DepartmentService { get; }
        private IDivisionService DivisionService { get; }
        private IEmployeeService EmployeeService { get; }
        private IGroupService GroupService { get; }
        private IRelationshipService RelationshipService { get; }
        private ILookupService LookupService { get; }

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
            IRelationshipService relationshipService,
            ILookupService lookupService)
        {
            Logger = logger;
            CacheServiceFactory = cacheServiceFactory;
            AuthenticationService = authenticationService;
            DepartmentService = departmentService;
            DivisionService = divisionService;
            EmployeeService = employeeService;
            GroupService = groupService;
            RelationshipService = relationshipService;
            LookupService = lookupService;
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
            LookupCache?.WriteCacheEntries();

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

            // login to the api
            Login(loginSettings);

            // setup a task to renew the token on a regular interval
            // authenticate with the c360 api
            _timer = new Timer(Login, loginSettings, loginInterval, loginInterval);

            // set the base address for the api services
            DepartmentService.SetBaseAddress(apiAddress);
            DivisionService.SetBaseAddress(apiAddress);
            EmployeeService.SetBaseAddress(apiAddress);
            GroupService.SetBaseAddress(apiAddress);
            RelationshipService.SetBaseAddress(apiAddress);
            LookupService.SetBaseAddress(apiAddress);

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
            LookupCache = CacheServiceFactory.CreateCacheService(Logger, "Lookup", false);
        }

        /// <summary>
        /// Called when the stream is complete and about to be closed.
        /// </summary>
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
                    //case SystemFields.JobTitleId:
                    //    value = GetJobTitleValue(map.From, user);
                    //    break;

                    case SystemFields.EmployeeNum:
                        employeeNum = GetFieldValueDefault(map.From, user);
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
                        relationships[map.Type] = GetFieldValueDefault(map.From, user);
                        break;
                        
                    default:
                        value = GetFieldValue(map.From, map.To, map.Type, user);
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

        /// <summary>
        /// Logs into the C360 API as the configured user.
        /// </summary>
        /// <param name="state"></param>
        private void Login(object state)
        {
            try
            {
                var loginSettings = (LoginSettings)state;

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
                var empId = EmployeeService.GetEmployeeByEmployeeNum(employeeNum, AuthToken);
                if (empId != null)
                {
                    employee.Id = empId.Id;
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

                var newEmployee = EmployeeService.CreateEmployeeAsync(employee, AuthToken).GetAwaiter().GetResult();
                employee.Id = newEmployee.Id;
                EmployeeCache.Add(employeeNum, newEmployee.Id);
            }
            else
            {
                // update an existing employee record
                EmployeeService.UpdateEmployee(employee, AuthToken);
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
            // get the relationships that belong to the employee
            // then use the id to fetch the details for each relationship
            var existingRelationships = RelationshipService.GetEmployeeRelationships(employee, AuthToken);

            var onlineRelations = new List<Relationship>();

            if (existingRelationships != null)
            {
                foreach (var relationshipId in existingRelationships)
                {
                    var relation =
                        RelationshipService.GetEmployeeRelationship(relationshipId, AuthToken);
                    onlineRelations.Add(relation);
                }
            }
            
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

                var relType = GetRelationshipType(relTypeName);

                // see if we can find the relationship in the online list
                bool processed = false;
                foreach (var onlineRel in onlineRelations)
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
                        RelationshipService.UpdateRelationship(onlineRel, new Entity { Id = destEmployeeId }, AuthToken);
                        processed = true;
                        break;
                    }
                }
                
                // not found create it
                if (!processed)
                {
                    var newRelId = RelationshipService.CreateRelationship(employee, new Entity { Id = destEmployeeId }, relType, AuthToken);
                }
            }
        }

        private Entity GetRelationshipType(string relTypeName)
        {
            Entity relType = null;
            if (RelationshipTypeCache.ContainsKey(relTypeName))
            {
                relType = new Entity {Id = RelationshipTypeCache.GetValue(relTypeName)};
            }
            else
            {
                var relationType = RelationshipService.GetRelationshipTypeByName(relTypeName, AuthToken);
                if (relationType != null)
                {
                    relType = relationType;
                    RelationshipTypeCache.Add(relTypeName, relationType.Id);
                }
                else
                {
                    relType = RelationshipService.CreateRelationshipType(relTypeName, AuthToken);
                    RelationshipTypeCache.Add(relTypeName, relType.Id);
                }
            }

            return relType;
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
            var workflowTemplate = EmployeeService.GetDefaultWorkflowTemplate(AuthToken);
            if (workflowTemplate != null)
            {
                WorkflowCache.Add(DefaultTemplateName, workflowTemplate.Id);
            }

            return workflowTemplate;
        }

        /// <summary>
        /// Gets a department id based on its name.
        /// </summary>
        /// <param name="from">The from value to parse</param>
        /// <param name="user">The user object that contains the data.</param>
        /// <param name="division"></param>
        /// <returns></returns>
        public Entity GetDepartment(
            string from, 
            ActiveDirectoryUser user, 
            Entity division)
        {
            // get the department name
            var departmentName = GetFieldValueDefault(from, user);
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
            var department = DepartmentService.GetDepartment(departmentName, division, AuthToken);
            if (department != null)
            {
                DepartmentCache.Add(departmentName, department.Id);
                return department;
            }

            // we did not find the department in the service...create it
            department = DepartmentService.CreateDepartment(departmentName, division, AuthToken);
            if (department != null)
            {
                DepartmentCache.Add(departmentName, department.Id);
                return department;
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
            var divisionName = GetFieldValueDefault(from, user);

            // check the cache
            if (DivisionCache.ContainsKey(divisionName))
            {
                return new Entity { Id = DivisionCache.GetValue(divisionName) };
            }

            // division does not exist in the cache...get it from the service
            var division = DivisionService.GetDivisionByName(divisionName, AuthToken);
            if (division != null)
            {
                DivisionCache.Add(divisionName, division.Id);
                return new Entity { Id = division.Id };
            }

            return division;
        }

        /// <summary>
        /// Gets the value from the user object
        /// </summary>
        /// <param name="from">The from value to parse</param>
        /// <param name="to">The destination field value</param>
        /// <param name="type">The optional type of field</param>
        /// <param name="user">The user object that contains the data.</param>
        /// <returns></returns>
        public object GetFieldValue(string from, string to, string type, ActiveDirectoryUser user)
        {
            if (string.IsNullOrEmpty(type))
            {
                return GetFieldValueDefault(from, user);
            }
            else if (type == FieldTypes.Lookup)
            {
                var lookupValue = GetFieldValueDefault(from, user);
                if (!string.IsNullOrEmpty(lookupValue))
                {
                    return GetLookupFieldValue(to, lookupValue);
                }
            }

            return null;
        }

        
        /// <summary>
        /// Gets the value from the user object
        /// </summary>
        /// <param name="from">The from value to parse</param>
        /// <param name="user">The user object that contains the data.</param>
        /// <returns></returns>
        public string GetFieldValueDefault(string from, ActiveDirectoryUser user)
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
        /// <param name="user">The user object that contains the data.</param>
        /// <returns></returns>
        public Entity GetJobTitleValue(string from, ActiveDirectoryUser user)
        {
            // get the field value for the job title name
            var jobTitleValue = GetFieldValueDefault(from, user);
            if (string.IsNullOrEmpty(jobTitleValue))
            {
                return null;
            }

            // check the cache 
            if (JobTitleCache.ContainsKey(jobTitleValue))
                return new Entity { Id = JobTitleCache.GetValue(jobTitleValue) };

            // job title does not exist in the cache...get it from the service
            var jobTitle = EmployeeService.GetJobTitle(jobTitleValue,AuthToken);
            if (jobTitle != null)
            {
                JobTitleCache.Add(jobTitleValue, jobTitle.Id);
                return jobTitle;
            }

            // did not find the job title in the system so add it.
            jobTitle = EmployeeService.CreateJobTitle(jobTitleValue, AuthToken);
            if (jobTitle != null)
            {
                JobTitleCache.Add(jobTitleValue, jobTitle.Id);
                return jobTitle;
            }

            return null;
        }

        /// <summary>
        /// Gets a Lookup value 
        /// </summary>
        /// <param name="lookupFieldName">The lookup field name.</param>
        /// <param name="lookupFieldValue">The lookup field value.</param>
        /// <returns></returns>
        public Entity GetLookupFieldValue(string lookupFieldName, string lookupFieldValue)
        {
            // check the cache 
            var cacheKey = $"{lookupFieldName}:{lookupFieldValue}";
            if (LookupCache.ContainsKey(cacheKey))
                return new Entity { Id = LookupCache.GetValue(cacheKey) };

            // lookup does not exist in the cache...try to get it
            var lookup = LookupService.GetLookupValue(lookupFieldName, lookupFieldValue, AuthToken);
            if (lookup != null)
            {
                LookupCache.Add(cacheKey, lookup.Id);
                return lookup;
            }

            // did not find the lookup in the system so add it.
            lookup = LookupService.CreateLookupValue(lookupFieldName, lookupFieldValue, AuthToken);
            if (lookup != null)
            {
                LookupCache.Add(cacheKey, lookup.Id);
                return lookup;
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
                var employeeProfile = EmployeeService.GetEmployeeProfile(new Entity{ Id = employee.Id }, AuthToken);
                profile = new Profile { Id = employeeProfile.Id };
            }

            // get the existing cached group membership
            var groupsWeAdded = new HashSet<string>();
            if (GroupMembershipCache.ContainsKey(employee.Id))
            {
                var ids = GroupMembershipCache.GetValue(employee.Id).Split(',');
                ids.ToList().ForEach(id =>
                {
                    if (!groupsWeAdded.Contains(id))
                    {
                        groupsWeAdded.Add(id);
                    }
                });
            }
           
            var onlineGroupRefs = GroupService.GetGroupMembership(profile, AuthToken);

            // we only get Ids back from the API so we need to further process them
            // so that we xref the Ids to the group names returned by AD
            var onlineGroups = ProcessOnlineGroups(onlineGroupRefs);

            var groupsToAdd = new List<Entity>();
            var adGroupIds = new List<string>();
            groupNames.ForEach(grpName =>
            {
                // lookup the group entity by name in the cache
                var groupId = EmployeeGroupCache.GetValue(grpName);
                var group = groupId != null ? new Entity {Id = groupId} : null;

                if (group == null)
                {
                    // try to get the group by its name using the api
                    group = GroupService.GetGroupByName(grpName, AuthToken);
                    if (group != null)
                    {
                        EmployeeGroupCache.Add(group.Id, grpName);
                    }
                }

                if (group == null)
                { 
                    // group is not in the system and needs to be created.
                    group = GroupService.CreateGroup(grpName, AuthToken);
                    EmployeeGroupCache.Add(group.Id, grpName);
                }

                // add the group to the list of AD groups...we will use this list to
                // figure out which groups need to be removed from Compliance 360
                adGroupIds.Add(group.Id);
                
                // if the user is not a current member of the group then
                // put it in the list to add to the profile
                if (!onlineGroups.ContainsKey(group.Id))
                {
                    groupsToAdd.Add(group);

                    groupsWeAdded.Add(group.Id);
                }
            });

            // check the list of online group Ids. If they are not in the list
            // of local AD groups and we have record that we added the user to the group them remove it
            var groupsToRemove = new List<Entity>();
            foreach (var onlineGroup in onlineGroupRefs)
            {
                if (!adGroupIds.Contains(onlineGroup.Id) && groupsWeAdded.Contains(onlineGroup.Id))
                {
                    groupsToRemove.Add(onlineGroup);

                    groupsWeAdded.Remove(onlineGroup.Id);
                }
            }

            EmployeeService.UpdateEmployeeProfile(profile, groupsToAdd, groupsToRemove, AuthToken);

            GroupMembershipCache.Add(employee.Id, String.Join(",", groupsWeAdded));
        }

        /// <summary>
        /// Process the list of group ids and ensure their names are in the 
        /// cache
        /// </summary>
        /// <param name="onlineGroups"></param>
        /// <returns>Dictionary of GroupId to names</returns>
        public Dictionary<string, string> ProcessOnlineGroups(List<Entity> onlineGroups)
        {
            var userGroups = new Dictionary<string, string>();

            // check cache for item
            foreach (var group in onlineGroups)
            {
                if (EmployeeGroupCache.ContainsKey(group.Id))
                {
                    userGroups[group.Id] = EmployeeGroupCache.GetValue(group.Id);
                }
                else
                {
                    // did not find it in cache...get name from api
                    var groupName = GroupService.GetGroupName(group, AuthToken);

                    // add it to the cache so that we do not have to 
                    // look it up again
                    EmployeeGroupCache.Add(group.Id, groupName);

                    userGroups[group.Id] = groupName;
                }
            }

            return userGroups;
        }
    }
}
