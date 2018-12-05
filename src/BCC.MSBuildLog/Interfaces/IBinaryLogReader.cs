extern alias StructuredLogger;
using System.Collections.Generic;
using Record = StructuredLogger::Microsoft.Build.Logging.Record;

namespace BCC.MSBuildLog.Interfaces
{
    public interface IBinaryLogReader
    {
        IEnumerable<Record> ReadRecords(string binLogPath);
    }
}