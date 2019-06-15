extern alias StructuredLogger;
using System.Collections.Generic;
using System.Linq;
using BCC.MSBuildLog.Legacy.MSBuild.Interfaces;
using Microsoft.Build.Framework;
using BinaryLogReplayEventSource = StructuredLogger::Microsoft.Build.Logging.BinaryLogReplayEventSource;
using Record = StructuredLogger::Microsoft.Build.Logging.Record;

namespace BCC.MSBuildLog.Legacy.MSBuild.Services
{
    public class BinaryLogReader : IBinaryLogReader
    {
        public IEnumerable<Record> ReadRecords(string binLogPath)
        {
            return new BinaryLogReplayEventSource()
                .ReadRecords(binLogPath)
                .Where(record => record.Args is BuildWarningEventArgs || record.Args is BuildErrorEventArgs)
                .OrderByDescending(record => record.Args is BuildErrorEventArgs);
        }
    }
}