using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Compliance360.EmployeeSync.Library.Configuration;
using Compliance360.EmployeeSync.Library.Data;
using Compliance360.EmployeeSync.Library.OutputStreams;
using Newtonsoft.Json;
using NLog;

namespace Compliance360.EmployeeSync.CsvStream
{
    public class CsvStream : IOutputStream
    {
        private ILogger Logger { get; }
        private JobElement JobConfig { get; set; }
        private StreamElement StreamConfig { get; set; }
        private StreamWriter CsvFile { get; set; }

        /// <summary>
        /// Initializes a new instance of the CsvStream
        /// </summary>
        /// <param name="logger">Instance of the logger.</param>
        public CsvStream(ILogger logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Closes the stream.
        /// </summary>
        public void Close()
        {
            if (CsvFile != null)
            {
                CsvFile.Flush();
                CsvFile.Close();
                CsvFile = null;
            }
            
            Logger.Info("Closed the Csv Stream");
        }
        public void StreamComplete()
        {

        }

        /// <summary>
        /// Opens the stream for writing.
        /// </summary>
        /// <param name="jobConfig">The current job configuration</param>
        /// <param name="streamConfig">The current stream configuration</param>
        public void Open(JobElement jobConfig, StreamElement streamConfig)
        {
            JobConfig = jobConfig;
            var csvPath = streamConfig.Settings["path"];
            CsvFile = new StreamWriter(new FileStream(csvPath, FileMode.Create, FileAccess.Write));

            // write the header labels row
            var first = true;
            foreach (AttributeElement attrib in jobConfig.Attributes)
            {
                if (!first)
                {
                    CsvFile.Write(",\"{0}\"", attrib.Name);
                }
                else
                {
                    first = false;
                    CsvFile.Write("\"{0}\"", attrib.Name);
                }
            }

            CsvFile.Flush();
        }

        /// <summary>
        /// Writes the user to the stream
        /// </summary>
        /// <param name="user">Reference to the user to write to the stream.</param>
        public void Write(ActiveDirectoryUser user)
        {
            CsvFile.Write("\n");

            var first = true;
            foreach (AttributeElement attrib in JobConfig.Attributes)
            {
                var value = string.Empty;
                var key = !string.IsNullOrEmpty(attrib.Alias) ? attrib.Alias : attrib.Name;
                if (user.Attributes.ContainsKey(key))
                {
                    var rawValue = user.Attributes[key];
                    if (rawValue is SortedList<string, string>)
                    {
                        foreach (var pair in (SortedList<string, string>) rawValue)
                        {
                            value += value.Length > 0 ?  $",{pair.Key}:{pair.Value}" : $"{pair.Key}:{pair.Value}";
                        }
                    }
                    else
                    {
                        value = rawValue?.ToString().Replace("\"", "\"\"");
                    }
                }

                if (!first)
                {
                    CsvFile.Write(",\"{0}\"", value);
                }
                else
                {
                    first = false;
                    CsvFile.Write("\"{0}\"", value);
                }
            }

            CsvFile.Flush();
        }
    }
}
