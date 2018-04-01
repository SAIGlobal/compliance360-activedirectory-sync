using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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
        private IApiService ApiService { get; }
        private ICacheServiceFactory CacheServiceFactory { get; }
        private ICacheService EmployeeCache { get; set; }
        private ICacheService DivisionCache { get; set; }
        private ICacheService JobTitleCache { get; set; }
        private ICacheService DepartmentCache { get; set; }
        private ICacheService WorkflowCache { get; set; }
        private ICacheService EmployeeGroupCache { get; set; }
        private ICacheService EmployeeProfileCache { get; set; }
        private System.Threading.Timer _timer;

        /// <summary>
        /// Initializes a new instance of the LoggerStream
        /// </summary>
        /// <param name="logger">Instance of the logger.</param>
        /// <param name="apiService">The C360 API service for accessing employee information.</param>
        /// <param name="cacheServiceFactory">Factory for returning cache service references.</param>
        public ApiStream(
            ILogger logger,
            IApiService apiService,
            ICacheServiceFactory cacheServiceFactory)
        {
            Logger = logger;
            ApiService = apiService;
            CacheServiceFactory = cacheServiceFactory;
        }

        /// <summary>
        /// Closes the stream and ends the api session
        /// </summary>
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

            Logger.Debug("Closed the Api Stream");
        }

        /// <summary>
        /// Opens the stream for writing
        /// </summary>
        /// <param name="jobConfig">The current job configuration.</param>
        /// <param name="streamConfig">The stream configuration.</param>
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
            
            Login(loginSettings);

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

                AuthToken = ApiService.LoginAsync(loginSettings.BaseAddress,
                    loginSettings.Organization,
                    loginSettings.Username,
                    loginSettings.Password).GetAwaiter().GetResult();
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
            var employee = new MetaObject();
            
            // process each of the field values in the map
            string employeeNum = null;
            EntityId workflowTemplate = null;
            EntityId division = null;
            SortedList<string, string> groups = null;

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

            if (workflowTemplate == null)
            {
                workflowTemplate = GetDefaultWorkflowTemplate();
                employee[SystemFields.WorkflowTemplate] = workflowTemplate;
            }
            
            // get the id of the employee from the cache...if found this is an update
            // otherwise we are creating a new employee.
            EntityId employeeId = null;
            if (EmployeeCache.ContainsKey(employeeNum))
            {
                employeeId = new EntityId(EmployeeCache.GetValue(employeeNum));
            }
            else
            {
                // check to see if the username already exists in c360
                var empId = ApiService.GetEmployeeIdAsync(employeeNum, AuthToken).GetAwaiter().GetResult();
                if (empId != null)
                {
                    employeeId = new EntityId(empId);
                    EmployeeCache.Add(employeeNum, employeeId.Token);
                }
            }

            if (employeeId == null)
            {
                // create the new employee record
                employee[SystemFields.CanLogin] = true;
                employee[SystemFields.Password] = Guid.NewGuid().ToString();
                
                employeeId = new EntityId(ApiService.CreateEmployeeAsync(employee, AuthToken).GetAwaiter().GetResult());
                EmployeeCache.Add(employeeNum, employeeId.Token);
            }
            else
            {
                // update an existing employee record
                ApiService.UpdateEmployeeAsync(employeeId, employee, AuthToken).GetAwaiter().GetResult();
            }

            if (groups != null)
            {
                ProcessGroupChanges(employeeId, groups.Values.ToList());
            }
        }
        
        /// <summary>
        /// Returns the default workflow template
        /// </summary>
        /// <returns></returns>
        public EntityId GetDefaultWorkflowTemplate()
        {
            const string DefaultTemplateName = "SystemDefault";

            // check the cache
            if (WorkflowCache.ContainsKey(DefaultTemplateName))
            {
                return new EntityId(WorkflowCache.GetValue(DefaultTemplateName));
            }

            // workflow does not exist in the cache. Get it from the service
            var workflowTemplateId = ApiService.GetDefaultWorkflowTemplateAsync(AuthToken).GetAwaiter().GetResult();
            if (workflowTemplateId != null)
            {
                WorkflowCache.Add(DefaultTemplateName, workflowTemplateId);
                return new EntityId(workflowTemplateId);
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
        public EntityId GetDepartment(string from, 
            ActiveDirectoryUser user, 
            EntityId division)
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
                return new EntityId(DepartmentCache.GetValue(departmentName));
            }

            // did not find the department in the cache
            // try to get it from the serice
            var departmentId = ApiService.GetDepartmentAsync(departmentName, division, AuthToken).GetAwaiter().GetResult();
            if (departmentId != null)
            {
                DepartmentCache.Add(departmentName, departmentId);
                return new EntityId(departmentId);
            }

            // we did not find the department in the service...create it
            departmentId = ApiService.CreateDepartmentAsync(departmentName, division, AuthToken).GetAwaiter().GetResult();
            if (departmentId != null)
            {
                DepartmentCache.Add(departmentName, departmentId);
                return new EntityId(departmentId);
            }

            return null;
        }

        /// <summary>
        /// Gets the division id value
        /// </summary>
        /// <param name="from">The from value to parse</param>
        /// <param name="user">The user object that contains the data.</param>
        /// <returns></returns>
        public EntityId GetDivisionFieldValue(string from, ActiveDirectoryUser user)
        {
            // get the division name value
            var divisionName = GetFieldValue(from, user);

            // check the cache
            if (DivisionCache.ContainsKey(divisionName))
            {
                return new EntityId(DivisionCache.GetValue(divisionName));
            }

            // division does not exist in the cache...get it from the service
            var divisionId = ApiService.GetDivisionAsync(divisionName, AuthToken).GetAwaiter().GetResult();
            if (divisionId != null)
            {
                DivisionCache.Add(divisionName, divisionId);
                return new EntityId(divisionId);
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
        public EntityId GetJobTitleValue(string from, EntityId division, ActiveDirectoryUser user)
        {
            // get the field value for the job title name
            var jobTitleValue = GetFieldValue(from, user);
            if (string.IsNullOrEmpty(jobTitleValue))
            {
                return null;
            }

            // check the cache 
            if (JobTitleCache.ContainsKey(jobTitleValue))
                return new EntityId(JobTitleCache.GetValue(jobTitleValue));

            // job title does not exist in the cache...get it from the service
            var jobTitleId = ApiService.GetJobTitleAsync(jobTitleValue, division, AuthToken).GetAwaiter().GetResult();
            if (jobTitleId != null)
            {
                JobTitleCache.Add(jobTitleValue, jobTitleId);
                return new EntityId(jobTitleId);
            }

            // did not find the job title in the system so add it.
            jobTitleId = ApiService.CreateJobTitleAsync(jobTitleValue, division, AuthToken).GetAwaiter().GetResult();
            if (jobTitleId != null)
            {
                JobTitleCache.Add(jobTitleValue, jobTitleId);
                return new EntityId(jobTitleId);
            }

            return null;
        }

        /// <summary>
        /// Processes the groups from AD and updates the users as needed.
        /// </summary>
        /// <param name="employeeId">The id of the employee</param>
        /// <param name="groupNames">The names of the groups the user is a member of</param>
        public void ProcessGroupChanges(EntityId employeeId, List<string> groupNames)
        {
            // get the id of the user's profile
            EntityId profileId = null;
            if (EmployeeProfileCache.ContainsKey(employeeId.Token))
            {
                profileId = new EntityId(EmployeeProfileCache.GetValue(employeeId.Token));
            }

            if (profileId == null)
            {
                // did not find it in the cache...get the id from the api
                var employeeProfileId = ApiService.GetEmployeeProfileIdAsync(employeeId, AuthToken).GetAwaiter().GetResult();
                profileId = new EntityId(employeeProfileId);
            }

            // get the existing group membership
            var onlineGroupsIds = ApiService.GetGroupMembershipAsync(profileId, AuthToken).GetAwaiter().GetResult();

            // we only get Ids back from the API so we need to further process them
            // so that we xref the Ids to the group names returned by AD
            var onlineGroups = ProcessOnlineGroups(onlineGroupsIds);

            // if not then add them to the user
            var groupsToAdd = new List<EntityId>();
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
                    groupsToAdd.Add(new EntityId(groupId));
                }
            });

            // check the list of online group Ids. If they are not in the list
            // of local AD groups them remove it
            var groupsToRemove = new List<EntityId>();
            foreach (var onlineGroup in onlineGroupsIds)
            {
                // bit of a hack but we do not want to remove the user from the 
                // division employees groups
                if (onlineGroups[onlineGroup].Contains("Employees"))
                    continue;

                if (!adGroupIds.Contains(onlineGroup))
                {
                    groupsToRemove.Add(new EntityId(onlineGroup));
                }
            }

            ApiService.UpdateEmployeeProfileAsync(profileId, groupsToAdd, groupsToRemove, AuthToken);
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
    }
}
