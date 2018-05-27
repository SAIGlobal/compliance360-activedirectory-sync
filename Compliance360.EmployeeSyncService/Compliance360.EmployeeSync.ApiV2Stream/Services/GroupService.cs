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
    public class GroupService : IGroupService
    {
        private ILogger Logger { get; }
        private IHttpDataService Http { get; }
        private Entity _groupsFolder;

        public GroupService(ILogger logger, IHttpDataService http)
        {
            Logger = logger;
            Http = http;
        }

        public void SetBaseAddress(string baseAddress)
        {
            Http.Initialize(baseAddress);
        }

        public async Task<Entity> CreateGroupAsync(string groupName, string token)
        {
            Logger.Debug("Creating group [{0}]", groupName);

            if (_groupsFolder == null)
            {
                _groupsFolder = await this.GetGroupsFolderAsync(token);
            }

            var createGroupUri = $"/API/2.0/Data/EmployeeManagement/EmployeeGroup/Default?token={token}";
            var group = new Dictionary<string, object>
            {
                {"GroupName", groupName},
                {"Dynamic", false},
                {"UseForSecurity", true},
                {"UseForWorkflow", true},
                {"Folder", _groupsFolder }
            };

            var result = await Http.PostAsync<CreateResponse>(createGroupUri, group);

            return new Entity {Id = result.Id};
        }

        public Entity CreateGroup(string groupName, string token)
        {
            return CreateGroupAsync(groupName, token).Result;
        }
        
        public async Task<Entity> GetGroupByNameAsync(string groupName, string token)
        {
            Logger.Debug("Getting Group [{0}]", groupName);

            var getGroupUri =
                $"/API/2.0/Data/EmployeeManagement/EmployeeGroup/Default?where=GroupName='{Uri.EscapeDataString(groupName)}'&take=1&token={token}";

            var resp = await Http.GetAsync<GetResponse<Entity>>(getGroupUri);

            var id = resp.Data?.FirstOrDefault()?.Id;
            return id != null ? new Entity {Id = id} : null;
        }

        public Entity GetGroupByName(string groupName, string token)
        {
            return GetGroupByNameAsync(groupName, token).Result;
        }

        public async Task<Entity> GetGroupsFolderAsync(string token)
        {
            Logger.Debug("Getting \"Groups\" root folder");

            var getGroupsFolderUri =
                $"/API/2.0/Data/Global/Folders/Default?select=Name,Parent,Division&where=Name='Groups'&token={token}";

            var resp = await Http.GetAsync<GetResponse<Folder>>(getGroupsFolderUri);

            var folder = resp.Data?.FirstOrDefault(f => f.Parent.Id == "NULL" && f.Division.Id == "NULL");

            return folder != null ? new Entity {Id = folder.Id} : null;
        }

        public Entity GetGroupsFolder(string token)
        {
            return GetGroupsFolderAsync(token).Result;
        }

        public async Task<List<Entity>> GetGroupMembershipAsync(Entity profile, string token)
        {
            Logger.Debug("Getting Group Membership for [{0}]", profile.Id);

            var getGroupsUri =
                $"/API/2.0/Data/EmployeeManagement/EmployeeProfile/Default?select=Groups&where=InstanceId='{profile.Id}'&token={token}";

            var resp = await Http.GetAsync<GetResponse<Profile>>(getGroupsUri);

            if (resp.Data?.Count > 0)
            {
                var groups = new List<Entity>();
                foreach (var group in resp.Data[0].Groups)
                {
                    groups.Add(new Entity{ Id = group.Id });
                }

                return groups;
            }

            return null;
        }
        
        public List<Entity> GetGroupMembership(Entity profile, string token)
        {
            return GetGroupMembershipAsync(profile, token).Result;
        }

        public async Task<string> GetGroupNameAsync(Entity group, string token)
        {
            Logger.Debug("Getting Group Name for [{0}]", group.Id);

            var getGroupUri =
                $"/API/2.0/Data/EmployeeManagement/EmployeeGroup/Default?select=GroupName&where=InstanceId='{group.Id}'&take=1&token={token}";

            var resp = await Http.GetAsync<GetResponse<EmployeeGroup>>(getGroupUri);

            return resp.Data?.FirstOrDefault()?.GroupName;
        }

        public string GetGroupName(Entity group, string token)
        {
            return GetGroupNameAsync(group, token).Result;
        }
    }
}
