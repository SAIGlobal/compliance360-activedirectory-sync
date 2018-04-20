using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using Compliance360.EmployeeSync.Library.Configuration;
using Compliance360.EmployeeSync.Library.Resources;
using Compliance360.EmployeeSync.Library.Filters;
using NLog;

namespace Compliance360.EmployeeSync.Library.Data
{
    public class ActiveDirectoryService : IActiveDirectoryService
    {
        public const string AttributeWhenChanged = "whenChanged";
        public const string AttributeDistinguishedName = "distinguishedName";
        private bool _disposedValue;

        public ILogger Logger { get; }
        private IUserFilterFactory UserFilterFactory { get; }
        private IAttributeFilterFactory AttributeFilterFactory { get; }

        /// <summary>
        ///     Initializes a new instance of the ActiveDirectory Service
        /// </summary>
        /// <param name="logger">Reference to the current configured logger</param>
        /// <param name="userFilterFactory">Factory for creating user filters</param>
        /// <param name="attributeFilterFactory">Factory for creating attribute filter</param>
        public ActiveDirectoryService(ILogger logger, IUserFilterFactory userFilterFactory,
            IAttributeFilterFactory attributeFilterFactory)
        {
            Logger = logger;
            UserFilterFactory = userFilterFactory;
            AttributeFilterFactory = attributeFilterFactory;
        }

        /// <summary>
        ///     Gets the LDAP users based on the job configuration.
        /// </summary>
        /// <param name="jobConfig">Job configuration</param>
        /// <returns>IEnumerable of ActiveDirectoryUser</returns>
        public IEnumerable<ActiveDirectoryUser> GetActiveDirectoryUsers(JobElement jobConfig)
        {
            var attribFilters = new Dictionary<string, IAttributeFilter>();
            var userFilters = new List<IUserFilter>
            {
                UserFilterFactory.CreateUserFilter(ContainerRegistry.PluginNameUserCacheFilter),
                UserFilterFactory.CreateUserFilter(ContainerRegistry.PluginNameUserGroupFilter)
            };

            var entry = BuildRootDirectoryEntry(jobConfig);

            // create the query string to find users that
            // are not exchange rooms
            Logger.Debug("Begining search for users: {0}", jobConfig.LdapQuery);
            var search = new DirectorySearcher(entry, jobConfig.LdapQuery);

            // once page size is set, all of the users that match the query will
            // be return a page at a time. The DirectorySearcher handles the 
            // return trips to the server to fetch the next page.
            search.PageSize = 1000;

            // setting this value to true tell the seacher to return
            // deleted entries
            search.Tombstone = true;

            var propertyNames = GetPropertyNamesForJob(jobConfig.Attributes);
            search.PropertiesToLoad.AddRange(propertyNames);

            using (var results = search.FindAll())
            {
                foreach (SearchResult result in results)
                {
                    var user = BuildActiveDirectoryUser(result, jobConfig, attribFilters);
                    string userName = null;
                    if (Logger.IsDebugEnabled)
                        userName =
                            $"{user.Attributes["givenName"]} {user.Attributes["sn"]} ({user.Attributes["sAMAccountName"]})";

                    foreach (var filter in userFilters)
                    { 
                        user = filter.Execute(user, jobConfig);
                        if (user == null)
                            break;
                    }

                    if (user != null)
                    {
                        Logger.Debug("{0} needs to be processed.", userName);
                        yield return user;
                    }
                    else
                    {
                        Logger.Debug("{0} did not meet the filter criteria or has not changed", userName);
                    }
                }
            }

            attribFilters.Values.ToList().ForEach(f => (f as IDisposable)?.Dispose());
            attribFilters.Clear();
            userFilters.ForEach(f => (f as IDisposable)?.Dispose());
            userFilters.Clear();
        }

        /// <summary>
        ///     Builds the root entry used to anchor the search.
        /// </summary>
        /// <param name="jobConfig">The current job configuration.</param>
        /// <returns>The configured directory entry</returns>
        private DirectoryEntry BuildRootDirectoryEntry(JobElement jobConfig)
        {
            // anchor the search at the specified domain / ou
            if (string.IsNullOrEmpty(jobConfig.Domain))
                throw new ArgumentException(ResourceManager.GetString("ErrorMissingDomain", jobConfig.Name));

            var baseAddress = GetBaseLdapAddress(jobConfig.Domain, jobConfig.Ou);
            var userName = jobConfig.Username;
            var password = jobConfig.Password;

            DirectoryEntry entry;
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
                entry = new DirectoryEntry(baseAddress, userName, password);
            else
                entry = new DirectoryEntry(baseAddress);
            Logger.Info("Connecting to {0}", baseAddress);
            return entry;
        }

        /// <summary>
        ///     Builds the active directory user based on the supplied result and job configuration.
        /// </summary>
        /// <param name="result">The LDAP search result.</param>
        /// <param name="jobConfig">The current job configuration.</param>
        /// <returns>Populated active directory user.</returns>
        public ActiveDirectoryUser BuildActiveDirectoryUser(SearchResult result,
            JobElement jobConfig,
            Dictionary<string, IAttributeFilter> attribFilters)
        {
            var user = new ActiveDirectoryUser();

            foreach (AttributeElement attrib in jobConfig.Attributes)
            {
                var attribName = string.IsNullOrEmpty(attrib.Alias) ? attrib.Name : attrib.Alias;
                user.Attributes[attribName] = ProcessAttribute(result, jobConfig, attrib, attribFilters);
            }

            return user;
        }

        /// <summary>
        ///     Processes and attribute and returns its value
        ///     to be stored in the ActiveDirectory user object
        /// </summary>
        /// <param name="result">The current result to process.</param>
        /// <param name="jobConfig">The current job configuration.</param>
        /// <param name="attrib">The attribute to process.</param>
        /// <param name="attribFilters">The dictionary of filters</param>
        /// <returns></returns>
        public object ProcessAttribute(SearchResult result,
            JobElement jobConfig,
            AttributeElement attrib,
            Dictionary<string, IAttributeFilter> attribFilters)
        {
            var filters = new List<IAttributeFilter>();
            if (attrib.IncludeInQuery)
                filters.Add(GetAttributeFilter(ContainerRegistry.PluginNameReadLdapFilter, attribFilters));

            attrib.Filter.Split(',').ToList().ForEach(f =>
            {
                if (!string.IsNullOrWhiteSpace(f))
                    filters.Add(GetAttributeFilter(f.Trim(), attribFilters));
            });

            object currentValue = null;
            foreach (var filter in filters)
                try
                {
                    currentValue = filter.Execute(currentValue, result, jobConfig, attrib);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error executing filter {0} for attribute {1}", filter.GetType().FullName,
                        attrib.Name);
                    throw;
                }

            return currentValue;
        }

        /// <summary>
        ///     Builds the list of property names to retreive as part of the LDAP query
        /// </summary>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public static string[] GetPropertyNamesForJob(AttributeElementCollection attributes)
        {
            var propertyNames = new List<string>();

            foreach (AttributeElement attrib in attributes)
                if (attrib.IncludeInQuery)
                    propertyNames.Add(attrib.Name);

            return propertyNames.ToArray();
        }

        /// <summary>
        ///     Creates the base LDAP address
        /// </summary>
        /// <param name="domain">The ldap domain</param>
        /// <param name="ou">The organization unit</param>
        /// <returns>Formatted LDAP address</returns>
        public static string GetBaseLdapAddress(string domain, string ou)
        {
            var address = new StringBuilder();
            address.Append("LDAP://");

            var addComma = false;
            if (!string.IsNullOrEmpty(ou))
            {
                address.Append(ou);
                addComma = true;
            }


            var segments = domain.Split('.');
            foreach (var seg in segments)
            {
                address.Append(addComma ? $",DC={seg}" : $"DC={seg}");
                if (!addComma)
                    addComma = true;
            }

            return address.ToString();
        }

        /// <summary>
        ///     Returns an attribute filter by name
        /// </summary>
        /// <param name="name">Name of the attribute filter to return.</param>
        /// <returns>Attribute filter</returns>
        public IAttributeFilter GetAttributeFilter(string name, Dictionary<string, IAttributeFilter> attribFilters)
        {
            IAttributeFilter filter;
            if (attribFilters.ContainsKey(name))
                filter = attribFilters[name];
            else
                try
                {
                    filter = AttributeFilterFactory.CreateAttributeFilter(name);
                    attribFilters.Add(name, filter);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Unable to get attribute filter [{0}]", name);
                    throw;
                }

            return filter;
        }
    }
}