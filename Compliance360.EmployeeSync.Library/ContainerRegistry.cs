using Compliance360.EmployeeSync.Library.Configuration;
using Compliance360.EmployeeSync.Library.Data;
using Compliance360.EmployeeSync.Library.Jobs;
using Compliance360.EmployeeSync.Library.Filters;
using Compliance360.EmployeeSync.Library.OutputStreams;
using NLog;
using StructureMap;

namespace Compliance360.EmployeeSync.Library
{
    public class ContainerRegistry : Registry
    {
        public const string PluginNameActiveDirectory = "ActiveDirectory";
        public const string PluginNameReadLdapFilter = "ReadLdapFilter";
        public const string PluginNameGroupsAttributeFilter = "GroupsAttributeFilter";
        public const string PluginNameUserGroupFilter = "UserGroupFilter";
        public const string PluginNameUserCacheFilter = "UserCacheFilter";
        public const string PluginNameUacAttributeFilter = "UacAttributeFilter";
        public const string PluginNameDomainAttributeFilter = "DomainAttributeFilter";
        public const string PluginNameSftpStream = "Sftp";
        public const string PluginNameLoggerStream = "Logger";
        //public const string PluginNameC360ApiV2Stream = "Compliance360ApiV2";

        public ContainerRegistry()
        {
            For<ISyncService>().Use<SyncService>();
            For<ILogger>().Use(() => LogManager.GetCurrentClassLogger());
            For<IConfigurationService>().Use<ConfigurationService>();
            For<ICacheServiceFactory>().Use<CacheServiceFactory>();
            For<IActiveDirectoryService>().Use<ActiveDirectoryService>().AlwaysUnique();
            For<IAttributeFilterFactory>().Use<AttributeFilterFactory>();
            For<IUserFilterFactory>().Use<UserFilterFactory>();
            For<IOutputStreamFactory>().Use<OutputStreamFactory>();
            
            // named jobs
            For<ISyncJob>().Use<ActiveDirectorySyncJob>().Named(PluginNameActiveDirectory).AlwaysUnique();

            // named attribute filters
            For<IAttributeFilter>().Use<ReadLdapFilter>().Named(PluginNameReadLdapFilter);
            For<IAttributeFilter>().Use<GroupsAttributeFilter>().Named(PluginNameGroupsAttributeFilter);
            For<IAttributeFilter>().Use<UacAttributeFilter>().Named(PluginNameUacAttributeFilter);
            For<IAttributeFilter>().Use<DomainAttributeFilter>().Named(PluginNameDomainAttributeFilter);

            // named user filters
            For<IUserFilter>().Use<UserGroupFilter>().Named(PluginNameUserGroupFilter);
            For<IUserFilter>().Use<UserCacheFilter>().Named(PluginNameUserCacheFilter).AlwaysUnique();

            // named streams
            For<IOutputStream>().Use<LoggerStream>().Named(PluginNameLoggerStream);
        }
    }
}