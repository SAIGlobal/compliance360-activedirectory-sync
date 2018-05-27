
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compliance360.EmployeeSync.Library.Configuration;

namespace Compliance360.EmployeeSync.Library.OutputStreams
{
    public class OutputStreamFactory : IOutputStreamFactory
    {
        public IOutputStream CreateOutputStream(JobElement jobConfig, StreamElement streamConfig)
        {
            var container = ContainerFactory.GetContainer();

            // get a reference to the stream object
            var stream = container.GetInstance<IOutputStream>(streamConfig.Name);

            // open the stream for writting
            stream.Open(jobConfig, streamConfig);

            return stream;
        }
    }
}
