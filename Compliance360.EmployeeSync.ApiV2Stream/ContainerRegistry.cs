using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compliance360.EmployeeSync.ApiV2Stream.Services;
using Compliance360.EmployeeSync.Library.OutputStreams;
using NLog;
using StructureMap;

namespace Compliance360.EmployeeSync.ApiV2Stream
{
    public class ContainerRegistry : Registry
    {
        public const string PluginNameC360ApiV2Stream = "Compliance360ApiV2";

        public ContainerRegistry()
        {
            For<IOutputStream>().Use<ApiStream>().Named(PluginNameC360ApiV2Stream).AlwaysUnique();
            For<IAuthenticationService>().Use<AuthenticationService>().AlwaysUnique();
            For<IDepartmentService>().Use<DepartmentService>().AlwaysUnique();
            For<IDivisionService>().Use<DivisionService>().AlwaysUnique();
            For<IEmployeeService>().Use<EmployeeService>().AlwaysUnique();
            For<IGroupService>().Use<GroupService>().AlwaysUnique();
            For<IRelationshipService>().Use<RelationshipService>().AlwaysUnique();
            For<ILookupService>().Use<LookupService>().AlwaysUnique();
            For<IHttpDataService>().Use<HttpDataService>().AlwaysUnique();
            For<IHttpClientHandler>().Use<HttpClientHandler>().AlwaysUnique();
        }
    }
}
