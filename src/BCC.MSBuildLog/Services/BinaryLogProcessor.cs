extern alias StructuredLogger;
using System;
using System.Diagnostics;
using BCC.MSBuildLog.Interfaces;
using BCC.MSBuildLog.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StructuredLogger::Microsoft.Build.Logging;

namespace BCC.MSBuildLog.Services
{
    public class BinaryLogProcessor : IBinaryLogProcessor
    {
        private readonly IBinaryLogReader _binaryLogReader;
        private ILogger<BinaryLogProcessor> Logger { get; }

        public BinaryLogProcessor(IBinaryLogReader binaryLogReader, ILogger<BinaryLogProcessor> logger = null)
        {
            _binaryLogReader = binaryLogReader;
            Logger = logger ?? new NullLogger<BinaryLogProcessor>();
        }

        /// <inheritdoc />
        public LogData ProcessLog(string binLogPath, string cloneRoot, string owner, string repo, string hash, CheckRunConfiguration configuration = null)
        {
            Logger.LogInformation("ProcessLog binLogPath:{0} cloneRoot:{1}", binLogPath, cloneRoot);

            return ProcessLogInternal(binLogPath, cloneRoot, owner, repo, hash, configuration, _binaryLogReader);
        }

        private static LogData ProcessLogInternal(string binLogPath, string cloneRoot, string owner, string repo, string hash,
            CheckRunConfiguration configuration, IBinaryLogReader binaryLogReader)
        {
            var logDataBuilder = new LogDataBuilder(cloneRoot, owner, repo, hash, configuration);

            foreach (var record in binaryLogReader.ReadRecords(binLogPath))
            {
                logDataBuilder.ProcessRecord(record.Args);
            }

            return logDataBuilder.Build();
        }
    }
}
