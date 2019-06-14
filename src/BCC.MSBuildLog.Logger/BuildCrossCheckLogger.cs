using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCC.Core.Model.CheckRunSubmission;
using BCC.MSBuildLog.Model;
using BCC.MSBuildLog.Services;
using BCC.Submission.Interfaces;
using BCC.Submission.Services;
using Microsoft.Build.Framework;
using RestSharp;

namespace BCC.MSBuildLog.Logger
{
    public class BuildCrossCheckLogger : Microsoft.Build.Utilities.Logger
    {
        internal bool BuildStarted { get; private set; }

        private ISubmissionService _submissionService;
        private LogDataBuilder _logDataBuilder;

        public override void Initialize(IEventSource eventSource)
        {
            Initialize(eventSource, new DefaultEnvironment(),
                new SubmissionService(new FileSystem(), new RestClient()));
        }

        internal void Initialize(IEventSource eventSource, IEnvironment environment,
            ISubmissionService submissionService)
        {
            var bccToken = environment.GetEnvironmentVariable("BCC_TOKEN");
            if (string.IsNullOrWhiteSpace(bccToken))
            {
                environment.WriteLine("BuildCrossCheck Token is not present");
                return;
            }

            environment.WriteLine("BuildCrossCheck Enabled");

            ParameterParser.Parse(Parameters);

            string cloneRoot = null;
            string owner = null;
            string repo = null;
            string hash = null;
            CheckRunConfiguration configuration = null;

            _logDataBuilder = new LogDataBuilder(cloneRoot, owner, repo, hash, configuration);
            _submissionService = submissionService;

            eventSource.BuildStarted += EventSourceOnBuildStarted;
            eventSource.ProjectStarted += EventSourceOnProjectStarted;
            eventSource.TargetStarted += EventSourceOnTargetStarted;
            eventSource.ProjectFinished += EventSourceOnProjectFinished;
            eventSource.BuildFinished += EventSourceOnBuildFinished;
            eventSource.MessageRaised += EventSourceOnMessageRaised;
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
        }

        private void EventSourceOnMessageRaised(object sender, BuildMessageEventArgs e)
        {
            GuardStarted();
        }

        private void EventSourceOnWarningRaised(object sender, BuildWarningEventArgs e)
        {
            GuardStarted();
        }

        private void EventSourceOnErrorRaised(object sender, BuildErrorEventArgs e)
        {
            GuardStarted();
        }

        void EventSourceOnProjectStarted(object sender, ProjectStartedEventArgs e)
        {
            GuardStarted();

            Console.WriteLine($"Project Started: {e.ProjectFile} {e.TargetNames}");
        }

        void EventSourceOnProjectFinished(object sender, ProjectFinishedEventArgs e)
        {
            GuardStarted();

            Console.WriteLine($"Project Finished: {e.ProjectFile}");
        }

        void EventSourceOnTargetStarted(object sender, TargetStartedEventArgs e)
        {
            GuardStarted();

            Console.WriteLine("Target Started: " + e.TargetName);
        }
    }
}