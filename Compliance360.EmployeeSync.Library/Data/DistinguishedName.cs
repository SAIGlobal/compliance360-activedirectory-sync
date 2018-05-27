using System.Collections.Generic;
using System.Text;

namespace Compliance360.EmployeeSync.Library.Data
{
    public sealed class DistinguishedName
    {
        /// <summary>
        ///     Initializes a new distinguished name
        /// </summary>
        /// <param name="distinguishedName">The Dn value</param>
        public DistinguishedName(string distinguishedName)
        {
            OrganizationUnits = new List<string>();
            DomainComponents = new List<string>();

            Dn = distinguishedName;

            ParseDistinguishedName(Dn);
        }

        public string Dn { get; set; }

        public string CommonName { get; set; }

        public List<string> OrganizationUnits { get; }

        public List<string> DomainComponents { get; }

        /// <summary>
        ///     Returns the DomainComponents as a dot separated string
        /// </summary>
        public string DomainName
        {
            get
            {
                var domain = new StringBuilder();
                foreach (var dc in DomainComponents)
                {
                    if (domain.Length > 0)
                        domain.Append(".");
                    domain.Append(dc);
                }
                return domain.ToString();
            }
        }

        /// <summary>
        ///     Returns the OrganizationUnits as a path string
        /// </summary>
        public string OrganizationPath
        {
            get
            {
                var path = new StringBuilder();
                foreach (var unit in OrganizationUnits)
                {
                    if (path.Length > 0)
                        path.Append("\\");
                    path.Append(unit);
                }
                return path.ToString();
            }
        }

        /// <summary>
        ///     Parses the Dn value into components
        /// </summary>
        /// <param name="dn">The distinguishedname value</param>
        private void ParseDistinguishedName(string dn)
        {
            var segments = dn.Split(',');
            foreach (var seg in segments)
            {
                var values = seg.Trim().Split('=');
                switch (values[0].ToLowerInvariant())
                {
                    case "cn":
                        CommonName = values[1];
                        break;

                    case "ou":
                        OrganizationUnits.Insert(0, values[1]);
                        break;

                    case "dc":
                        DomainComponents.Add(values[1]);
                        break;
                }
            }
        }
    }
}