namespace Compliance360.EmployeeSync.Library.Resources
{
    /// <summary>
    ///     This class is the resource manager for the LDAP Sync library
    /// </summary>
    public static class ResourceManager
    {
        private static readonly System.Resources.ResourceManager Resources =
            new System.Resources.ResourceManager(
                "Compliance360.EmployeeSync.Library.Resources.Resources",
                typeof(ResourceManager).Assembly);

        /// <summary>
        ///     Gets a string from the resource manager.
        /// </summary>
        /// <param name="key">The key of the resource to return.</param>
        /// <param name="args">Optional string format arguments.</param>
        /// <returns>Resource string.</returns>
        public static string GetString(string key, params object[] args)
        {
            if (args != null && args.Length > 0)
            {
                var value = Resources.GetString(key);
                return value != null ? string.Format(value, args) : null;
            }

            return Resources.GetString(key);
        }
    }
}