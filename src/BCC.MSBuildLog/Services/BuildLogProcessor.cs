using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using BCC.Core.Extensions;
using BCC.Core.Model.CheckRunSubmission;
using BCC.MSBuildLog.Interfaces;
using BCC.MSBuildLog.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace BCC.MSBuildLog.Services
{
    public class BuildLogProcessor : IBuildLogProcessor
    {
        private readonly IFileSystem _fileSystem;
        private readonly IBinaryLogProcessor _binaryLogProcessor;

        private ILogger<BuildLogProcessor> Logger { get; }

        public BuildLogProcessor(
            IFileSystem fileSystem,
            IBinaryLogProcessor binaryLogProcessor,
            ILogger<BuildLogProcessor> logger = null)
        {
            _fileSystem = fileSystem;
            _binaryLogProcessor = binaryLogProcessor;

            Logger = logger ?? new NullLogger<BuildLogProcessor>();
        }

        public void Process(string inputFile, string outputFile, string cloneRoot, string owner, string repo, string hash, string configurationFile = null)
        {
            if (!_fileSystem.File.Exists(inputFile))
            {
                throw new InvalidOperationException($"Input file `{inputFile}` does not exist.");
            }

            if (_fileSystem.File.Exists(outputFile))
            {
                throw new InvalidOperationException($"Output file `{outputFile}` already exists.");
            }

            CheckRunConfiguration configuration = null;
            if (configurationFile != null)
            {
                if (!_fileSystem.File.Exists(configurationFile))
                {
                    throw new InvalidOperationException($"Configuration file `{configurationFile}` does not exist.");
                }

                var configurationString = _fileSystem.File.ReadAllText(configurationFile);
                if (string.IsNullOrWhiteSpace(configurationString))
                {
                    throw new InvalidOperationException($"Content of configuration file `{configurationFile}` is null or empty.");
                }

                configuration = JsonConvert.DeserializeObject<CheckRunConfiguration>(configurationString, new JsonSerializerSettings
                {
                    Formatting = Formatting.None,
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Converters = new List<JsonConverter>
                    {
                        new StringEnumConverter { NamingStrategy = new CamelCaseNamingStrategy() }
                    },
                    MissingMemberHandling = MissingMemberHandling.Error
                });
            }

            var dateTimeOffset = DateTimeOffset.Now;
            var logData = _binaryLogProcessor.ProcessLog(inputFile, cloneRoot, owner, repo, hash, configuration);

            var hasAnyFailure = logData.Annotations.Any() &&
                                logData.Annotations.Any(annotation => annotation.AnnotationLevel == AnnotationLevel.Failure);

            var stringBuilder = new StringBuilder();
            stringBuilder.Append(logData.ErrorCount.ToString());
            stringBuilder.Append(" ");
            stringBuilder.Append(logData.ErrorCount == 1 ? "error": "errors");
            stringBuilder.Append(" - ");
            stringBuilder.Append(logData.WarningCount.ToString());
            stringBuilder.Append(" ");
            stringBuilder.Append(logData.WarningCount == 1 ? "warning" : "warnings");

            var createCheckRun = new CreateCheckRun
            {
                Annotations = logData.Annotations,
                Conclusion = !hasAnyFailure ? CheckConclusion.Success : CheckConclusion.Failure,
                StartedAt = dateTimeOffset,
                CompletedAt = DateTimeOffset.Now,
                Summary = logData.Report,
                Name = configuration?.Name ?? "MSBuild Log",
                Title = stringBuilder.ToString(),
            };

            var contents = createCheckRun.ToJson();

            _fileSystem.File.WriteAllText(outputFile, contents);
        }
    }
}