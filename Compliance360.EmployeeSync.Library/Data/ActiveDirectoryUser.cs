using System.Collections.Generic;
using System.Collections;

namespace Compliance360.EmployeeSync.Library.Data
{
    public class ActiveDirectoryUser
    {
        /// <summary>
        ///     Initializes a new instance of the ActiveDirectoryUser
        /// </summary>
        public ActiveDirectoryUser()
        {
            Attributes = new SortedList<string, object>();
        }

        public SortedList<string, object> Attributes { get; }
    }
}