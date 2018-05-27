using System.Collections.Generic;
using Compliance360.EmployeeSync.Library.Configuration;
using Compliance360.EmployeeSync.Library.Data;

namespace Compliance360.EmployeeSync.Library.Filters
{
    /// <summary>
    ///     This filter filters out users that do not match the filterGroupPrefix
    ///     defined in the job config
    /// </summary>
    public class UserGroupFilter : IUserFilter
    {
        public ActiveDirectoryUser Execute(ActiveDirectoryUser user, JobElement jobConfig)
        {
            if (jobConfig.Groups.Count == 0 ||
                !user.Attributes.ContainsKey("memberOf") ||
                !(user.Attributes["memberOf"] is SortedList<string, string>))
                return user;

            // since the "allowedGroups" are present, the user is allowed if
            // it has at least one group in the list
            var groups = (SortedList<string, string>) user.Attributes["memberOf"];
            if (groups.Count > 0)
                return user;

            return null;
        }
    }
}