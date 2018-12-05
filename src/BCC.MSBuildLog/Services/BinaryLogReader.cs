extern alias StructuredLogger;
using System.Collections.Generic;
using System.Linq;
using BCC.MSBuildLog.Interfaces;
using Microsoft.Build.Framework;
using StructuredLogger::Microsoft.Build.Logging;

namespace BCC.MSBuildLog.Services
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