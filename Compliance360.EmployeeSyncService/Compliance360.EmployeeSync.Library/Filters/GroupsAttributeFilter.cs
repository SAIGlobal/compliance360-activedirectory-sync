using System.Collections.Generic;
using System.DirectoryServices;
using Compliance360.EmployeeSync.Library.Data;
using Compliance360.EmployeeSync.Library.Configuration;

namespace Compliance360.EmployeeSync.Library.Filters
{
    public class GroupsAttributeFilter : IAttributeFilter
    {
        public object Execute(object currentValue, SearchResult result, JobElement jobConfig, AttributeElement attrib)
        {
            // build the list of allowed groups...if populated
            // then the group will only be included if it matches
            // one of the allowed group names
            var groups = new SortedList<string, string>();
            var groupSet = new HashSet<string>();
            foreach (GroupElement group in jobConfig.Groups)
                groupSet.Add(group.Name);

            // get the current member values
            var props = result.Properties[attrib.Name];
            if (props == null)
            {
                return null;
            }

            foreach (string dn in props)
            {
                var group = new DistinguishedName(dn);

                // check to see if the group is in the allowed list
                // or if the list is empty then allow the group
                var groupName = string.IsNullOrWhiteSpace(jobConfig.RemoveGroupPrefix) ? 
                    group.CommonName :
                    group.CommonName.Replace(jobConfig.RemoveGroupPrefix, string.Empty);

                if (groupSet.Count == 0 || groupSet.Contains(groupName))
                {
                    groups[dn] = groupName;
                }
            }

            return groups;
        }
    }
}