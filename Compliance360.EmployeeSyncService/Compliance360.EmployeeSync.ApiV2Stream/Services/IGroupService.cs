using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compliance360.EmployeeSync.ApiV2Stream.Data;
using Newtonsoft.Json.Linq;

namespace Compliance360.EmployeeSync.ApiV2Stream.Services
{
    public interface IGroupService
    {
        /// <summary>
        /// Sets the base uri of the Http client.
        /// </summary>
        /// <param name="baseAddress">The bsae address of the API.</param>
        void SetBaseAddress(string baseAddress);

        /// <summary>
        /// Creates a new group
        /// </summary>
        /// <param name="groupName">Name of the new group.</param>
        /// <param name="token">Auth token.</param>
        /// <returns>Entity reference to the group.</returns>
        Task<Entity> CreateGroupAsync(string groupName, string token);

        /// <summary>
        /// Creates a new group
        /// </summary>
        /// <param name="groupName">Name of the new group.</param>
        /// <param name="token">Auth token.</param>
        /// <returns>Entity reference to the group.</returns>
        Entity CreateGroup(string groupName, string token);

        /// <summary>
        /// Gets a group based on its name
        /// </summary>
        /// <param name="groupName">Name of the group to find.</param>
        /// <param name="token">Current auth token.</param>
        /// <returns>Entity reference to the group.</returns>
        Task<Entity> GetGroupAsync(string groupName, string token);

        /// <summary>
        /// Gets a group based on its name
        /// </summary>
        /// <param name="groupName">Name of the group to find.</param>
        /// <param name="token">Current auth token.</param>
        /// <returns>Entity reference to the group.</returns>
        Entity GetGroup(string groupName, string token);

        /// <summary>
        /// Gets the Id of the "Groups" folder where the
        /// groups will be created.
        /// </summary>
        /// <param name="token">Auth token</param>
        /// <returns>Entity reference to the folder.</returns>
        Task<Entity> GetGroupsFolderAsync(string token);

        /// <summary>
        /// Gets the Id of the "Groups" folder where the
        /// groups will be created.
        /// </summary>
        /// <param name="token">Auth token</param>
        /// <returns>Entity reference to the folder.</returns>
        Entity GetGroupsFolder(string token);

        /// <summary>
        /// Returns a dictionary of groups for which the user is a member.
        /// </summary>
        /// <param name="profile">The local profile id.</param>
        /// <param name="token">The auth token.</param>
        /// <returns>List of Entity group references</returns>
        Task<List<Entity>> GetGroupMembershipAsync(Entity profile, string token);

        /// <summary>
        /// Returns a dictionary of groups for which the user is a member.
        /// </summary>
        /// <param name="profile">The local profile id.</param>
        /// <param name="token">The auth token.</param>
        /// <returns>List of Entity group references</returns>
        List<Entity> GetGroupMembership(Entity profile, string token);

        /// <summary>
        /// Gets a group name based on its Id
        /// </summary>
        /// <param name="group">Id of the group to find.</param>
        /// <param name="token">Auth token</param>
        /// <returns>Name of the group</returns>
        Task<string> GetGroupNameAsync(Entity group, string token);

        /// <summary>
        /// Gets a group name based on its Id
        /// </summary>
        /// <param name="group">Id of the group to find.</param>
        /// <param name="token">Auth token</param>
        /// <returns>Name of the group</returns>
        string GetGroupName(Entity group, string token);
    }
}
