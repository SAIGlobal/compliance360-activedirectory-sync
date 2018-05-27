using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compliance360.EmployeeSync.Library.OutputStreams;
using NLog;
using StructureMap;

namespace Compliance360.EmployeeSync.CsvStream
{
    public class ContainerRegistry : Registry
    {
        public const string PluginNameC360CsvStream = "Csv";

        public ContainerRegistry()
        {
            For<IOutputStream>().Use<CsvStream>().Named(PluginNameC360CsvStream).AlwaysUnique();
            For<CsvStream>().Use<CsvStream>().AlwaysUnique();
        }
    }
}
