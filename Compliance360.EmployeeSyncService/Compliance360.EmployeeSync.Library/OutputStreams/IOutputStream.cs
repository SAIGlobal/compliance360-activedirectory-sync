using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compliance360.EmployeeSync.Library.Configuration;
using Compliance360.EmployeeSync.Library.Data;

namespace Compliance360.EmployeeSync.Library.OutputStreams
{
    public interface IOutputStream
    {
        void Open(JobElement jobConfig, StreamElement streamConfig);

        void Close();

        void Write(ActiveDirectoryUser user);
    }
}
