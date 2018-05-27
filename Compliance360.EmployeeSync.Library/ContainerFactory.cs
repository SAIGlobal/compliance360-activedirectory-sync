using StructureMap;

namespace Compliance360.EmployeeSync.Library
{
    /// <summary>
    ///     This class provides global access to the configured IoC
    ///     container for the application.
    /// </summary>
    public static class ContainerFactory
    {
        private static Container _container;

        public static Container GetContainer(Registry registry = null)
        {
            // if we are being passed a new registry
            // then reset the the container
            if (registry != null)
            {
                Reset();
                _container = new Container(registry);
                return _container;
            }

            if (_container != null)
            {
                return _container;
            }

            // populate the container by scanning for registries
            // within assemblies in the application bin folder
            _container = new Container(_ =>
            {
                _.Scan(x =>
                {
                    x.AssembliesFromApplicationBaseDirectory();
                    x.LookForRegistries();
                });
            });

            return _container;
        }

        public static void Reset()
        {
            if (_container != null)
            {
                _container.Dispose();
                _container = null;
            }
        }
    }
}