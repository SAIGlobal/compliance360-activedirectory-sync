using Compliance360.EmployeeSync.Library;
using Topshelf;

namespace Compliance360.EmployeeSyncService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var container = ContainerFactory.GetContainer();

            HostFactory.Run(h =>
            {
                h.Service<ISyncService>(s =>
                {
                    s.ConstructUsing(name => container.GetInstance<ISyncService>());
                    s.WhenStarted(svc => svc.Start());
                    s.WhenStopped(svc => svc.Stop());
                });
                h.StartManually();
                h.SetDisplayName("Compliance 360 LDAP Sync Service");
                h.SetDescription(
                    "This service synchronizes employee information from Active Directory (LDAP) with the Compliance 360 application.");
                h.SetServiceName("C360EmployeeSyncService");
                h.RunAsNetworkService();
            });
        }
    }
}