using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Framework;

namespace BCC.MSBuildLog.Logger
{
    public class MySimpleLogger : Microsoft.Build.Utilities.Logger
    {
        public override void Initialize(IEventSource eventSource)
        {
            //Register for the ProjectStarted, TargetStarted, and ProjectFinished events
            eventSource.BuildStarted += EventSourceOnBuildStarted;
            eventSource.ProjectStarted += EventSourceOnProjectStarted;
            eventSource.TargetStarted += EventSourceOnTargetStarted;
            eventSource.ProjectFinished += EventSourceOnProjectFinished;
            eventSource.BuildFinished += EventSourceOnBuildFinished;
            eventSource.MessageRaised += EventSourceOnMessageRaised;
            eventSource.WarningRaised += EventSourceOnWarningRaised;
            eventSource.ErrorRaised += EventSourceOnErrorRaised;
        }

        private void EventSourceOnBuildFinished(object sender, BuildFinishedEventArgs e)
        {
            
        }

        private void EventSourceOnBuildStarted(object sender, BuildStartedEventArgs e)
        {
            
        }

        private void EventSourceOnMessageRaised(object sender, BuildMessageEventArgs e)
        {
            
        }

        private void EventSourceOnWarningRaised(object sender, BuildWarningEventArgs e)
        {
            
        }

        private void EventSourceOnErrorRaised(object sender, BuildErrorEventArgs e)
        {

        }

        void EventSourceOnProjectStarted(object sender, ProjectStartedEventArgs e)
        {
            Console.WriteLine($"Project Started: {e.ProjectFile} {e.TargetNames}");
        }

        void EventSourceOnProjectFinished(object sender, ProjectFinishedEventArgs e)
        {
            Console.WriteLine($"Project Finished: {e.ProjectFile}");
        }

        void EventSourceOnTargetStarted(object sender, TargetStartedEventArgs e)
        {
            Console.WriteLine("Target Started: " + e.TargetName);
        }
    }
}
