using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compliance360.EmployeeSync.Library.Configuration;

namespace Compliance360.EmployeeSync.Library.OutputStreams
{
    public interface IOutputStreamFactory
    {
        IOutputStream CreateOutputStream(JobElement jobConfig, StreamElement streamConfig);
    }
}
