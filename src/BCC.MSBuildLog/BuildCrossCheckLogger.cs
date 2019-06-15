using System;
using System.Collections.Generic;
using System.IO.Abstractions;
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
        internal bool BuildStarted { get; private set; }

        private ISubmissionService _submissionService;
        private ILogDataBuilder _logDataBuilder;

        public override void Initialize(IEventSource eventSource)
        {
            var environmentProvider = new EnvironmentProvider();
            var fileSystem = new FileSystem();
            var restClient = new RestClient();
            var submissionService = new SubmissionService(fileSystem, restClient);

            var buildServiceProvider = new BuildServiceProvider(environmentProvider);
            var buildService = buildServiceProvider.GetBuildService();
            var parameterParser = new ParameterParser(environmentProvider, buildService);

            var logDataBuilderFactory = new LogDataBuilderFactory();

            Initialize(fileSystem, eventSource, environmentProvider, submissionService, parameterParser, logDataBuilderFactory);
        }

        internal void Initialize(IFileSystem fileSystem, IEventSource eventSource,
            IEnvironmentProvider environmentProvider, ISubmissionService submissionService,
            IParameterParser parameterParser, ILogDataBuilderFactory logDataBuilderFactory)
        {
            environmentProvider.WriteLine("BuildCrossCheck Enabled");

            var parameters = parameterParser.Parse(Parameters);

            if (string.IsNullOrWhiteSpace(parameters.Token))
            {
                environmentProvider.WriteLine("BuildCrossCheck Token is not present");
                return;
            }

            var configuration = LoadCheckRunConfiguration(fileSystem, parameters.ConfigurationFile);
            _logDataBuilder = logDataBuilderFactory.BuildLogDataBuilder(parameters, configuration);

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

        private void GuardStopped()
        {
            if (BuildStarted)
                throw new InvalidOperationException("Build already started");
        }

        private void GuardStarted()
        {
            if (!BuildStarted)
                throw new InvalidOperationException("Build not started");
        }

        private void EventSourceOnBuildStarted(object sender, BuildStartedEventArgs e)
        {
            GuardStopped();

            BuildStarted = true;
        }

        private void EventSourceOnBuildFinished(object sender, BuildFinishedEventArgs e)
        {
            GuardStarted();

            BuildStarted = false;
        }

        private void EventSourceOnWarningRaised(object sender, BuildWarningEventArgs e)
        {
            GuardStarted();
        }

        private void EventSourceOnErrorRaised(object sender, BuildErrorEventArgs e)
        {
            GuardStarted();
        }
    }
}