using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using BCC.Core.Extensions;
using BCC.Core.Model.CheckRunSubmission;
using BCC.MSBuildLog.Interfaces;
using BCC.MSBuildLog.Model;
using BCC.MSBuildLog.Services;
using BCC.MSBuildLog.Services.Build;
using Microsoft.Build.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using RestSharp;

namespace BCC.MSBuildLog
{ 
    public class BuildCrossCheckLogger : Microsoft.Build.Utilities.Logger
    {
        private ISubmissionService _submissionService;
        private ILogDataBuilder _logDataBuilder;
        private CheckRunConfiguration _configuration;
        private DateTimeOffset _startedAt;
        private Parameters _parameters;
        private IEnvironmentProvider _environmentProvider;

        public override void Initialize(IEventSource eventSource)
        {
            var environmentProvider = new EnvironmentProvider();

            var baseUrl = environmentProvider.GetEnvironmentVariable("BCC_URL") ?? "https://buildcrosscheck.azurewebsites.net";
            var restClient = new RestClient(baseUrl);
            var submissionService = new SubmissionService(restClient);

            var buildServiceProvider = new BuildServiceProvider(environmentProvider);
            var buildService = buildServiceProvider.GetBuildService();
            var parameterParser = new ParameterParser(environmentProvider, buildService);

            var logDataBuilderFactory = new LogDataBuilderFactory();
            var fileSystem = new FileSystem();

            Initialize(fileSystem, eventSource, environmentProvider, submissionService, parameterParser, logDataBuilderFactory);
        }

        internal void Initialize(IFileSystem fileSystem, IEventSource eventSource,
            IEnvironmentProvider environmentProvider, ISubmissionService submissionService,
            IParameterParser parameterParser, ILogDataBuilderFactory logDataBuilderFactory)
        {
            _environmentProvider = environmentProvider;
            _parameters = parameterParser.Parse(Parameters);

            if (string.IsNullOrWhiteSpace(_parameters.Token))
            {
                _environmentProvider.WriteLine("BuildCrossCheck Token is not present");
                return;
            }

            if (string.IsNullOrWhiteSpace(_parameters.CloneRoot))
            {
                _environmentProvider.WriteLine("BuildCrossCheck CloneRoot is not present");
                return;
            }

            if (string.IsNullOrWhiteSpace(_parameters.Owner))
            {
                _environmentProvider.WriteLine("BuildCrossCheck Owner is not present");
                return;
            }

            if (string.IsNullOrWhiteSpace(_parameters.Repo))
            {
                _environmentProvider.WriteLine("BuildCrossCheck Repo is not present");
                return;
            }

            if (string.IsNullOrWhiteSpace(_parameters.Hash))
            {
                _environmentProvider.WriteLine("BuildCrossCheck Hash is not present");
                return;
            }

            _environmentProvider.WriteLine("BuildCrossCheck Enabled");

            _configuration = LoadCheckRunConfiguration(fileSystem, _parameters.ConfigurationFile);
            _logDataBuilder = logDataBuilderFactory.BuildLogDataBuilder(_parameters, _configuration);

            _submissionService = submissionService;

            eventSource.BuildStarted += EventSourceOnBuildStarted;
            eventSource.BuildFinished += EventSourceOnBuildFinished;
            eventSource.WarningRaised += EventSourceOnWarningRaised;
            eventSource.ErrorRaised += EventSourceOnErrorRaised;
        }

        private CheckRunConfiguration LoadCheckRunConfiguration(IFileSystem fileSystem, string configurationFile)
        {
            CheckRunConfiguration configuration = null;
            if (configurationFile != null)
            {
                if (!fileSystem.File.Exists(configurationFile))
                {
                    throw new InvalidOperationException($"Configuration file `{configurationFile}` does not exist.");
                }

                var configurationString = fileSystem.File.ReadAllText(configurationFile);
                if (string.IsNullOrWhiteSpace(configurationString))
                {
                    throw new InvalidOperationException(
                        $"Content of configuration file `{configurationFile}` is null or empty.");
                }

                configuration = JsonConvert.DeserializeObject<CheckRunConfiguration>(configurationString,
                    new JsonSerializerSettings
                    {
                        Formatting = Formatting.None,
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                        Converters = new List<JsonConverter>
                        {
                            new StringEnumConverter {NamingStrategy = new CamelCaseNamingStrategy()}
                        },
                        MissingMemberHandling = MissingMemberHandling.Error
                    });
            }

            return configuration;
        }

        private void EventSourceOnBuildStarted(object sender, BuildStartedEventArgs e)
        {
            _startedAt = DateTimeOffset.Now;
        }

        private void EventSourceOnBuildFinished(object sender, BuildFinishedEventArgs e)
        {
            var submitSuccess = false;

            try
            {
                var logData = _logDataBuilder.Build();

                var hasAnyFailure = logData.Annotations.Any() &&
                                    logData.Annotations.Any(annotation => annotation.AnnotationLevel == AnnotationLevel.Failure);

                var stringBuilder = new StringBuilder();
                stringBuilder.Append(logData.ErrorCount.ToString());
                stringBuilder.Append(" ");
                stringBuilder.Append(logData.ErrorCount == 1 ? "error" : "errors");
                stringBuilder.Append(" - ");
                stringBuilder.Append(logData.WarningCount.ToString());
                stringBuilder.Append(" ");
                stringBuilder.Append(logData.WarningCount == 1 ? "warning" : "warnings");

                var createCheckRun = new CreateCheckRun
                {
                    Annotations = logData.Annotations,
                    Conclusion = !hasAnyFailure ? CheckConclusion.Success : CheckConclusion.Failure,
                    StartedAt = _startedAt,
                    CompletedAt = DateTimeOffset.Now,
                    Summary = logData.Report,
                    Name = _configuration?.Name ?? "MSBuild Log",
                    Title = stringBuilder.ToString(),
                };

                var contents = createCheckRun.ToJson();
                var bytes = Encoding.Unicode.GetBytes(contents);
                submitSuccess = _submissionService.SubmitAsync(bytes, _parameters).Result;
            }
            catch (Exception exception)
            {
                _environmentProvider.WriteLine(exception.ToString());
            }
            _environmentProvider.WriteLine($"Submission {(submitSuccess ? "Success" : "Failure")}");
            
        }

        private void EventSourceOnWarningRaised(object sender, BuildWarningEventArgs e)
        {
            _logDataBuilder.ProcessRecord(e);
        }

        private void EventSourceOnErrorRaised(object sender, BuildErrorEventArgs e)
        {
            _logDataBuilder.ProcessRecord(e);
        }
    }
}