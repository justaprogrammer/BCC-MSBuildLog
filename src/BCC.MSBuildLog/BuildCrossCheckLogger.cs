using System;
using System.IO.Abstractions;
using BCC.MSBuildLog.Interfaces;
using BCC.MSBuildLog.Legacy.MSBuild.Interfaces;
using BCC.MSBuildLog.Legacy.MSBuild.Services;
using BCC.MSBuildLog.Legacy.Submission.Interfaces;
using BCC.MSBuildLog.Legacy.Submission.Services;
using BCC.MSBuildLog.Services;
using BCC.MSBuildLog.Services.Build;
using Microsoft.Build.Framework;
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

            var configuration = BuildLogProcessor.LoadCheckRunConfiguration(fileSystem, parameters.ConfigurationFile);
            _logDataBuilder = logDataBuilderFactory.BuildLogDataBuilder(parameters, configuration);

            _submissionService = submissionService;

            eventSource.BuildStarted += EventSourceOnBuildStarted;
            eventSource.BuildFinished += EventSourceOnBuildFinished;
            eventSource.WarningRaised += EventSourceOnWarningRaised;
            eventSource.ErrorRaised += EventSourceOnErrorRaised;
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