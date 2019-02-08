using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compliance360.EmployeeSync.Library.Configuration;
using Compliance360.EmployeeSync.Library.Data;

namespace Compliance360.EmployeeSync.Library.OutputStreams
{
    public class ErrorGeneratorStream : IOutputStream 
    {
        public void Open(JobElement jobConfig, StreamElement streamConfig)
        {

        }

        public void StreamComplete()
        {

        }

        public void Close()
        {

        }

        public void Write(ActiveDirectoryUser user)
        {
            throw new DataException("Error Generator - Failed to process user");
        }
    }
}
