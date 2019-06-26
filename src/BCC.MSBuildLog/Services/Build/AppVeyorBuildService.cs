using System;
using System.Linq;
using BCC.MSBuildLog.Interfaces;

namespace BCC.MSBuildLog.Services.Build
{
    /// <summary>
    /// Wrapper of AppVeyor environment information
    /// https://www.appveyor.com/docs/environment-variables/
    /// </summary>
    public class AppVeyorBuildService : BuildServiceBase
    {
        public AppVeyorBuildService(IEnvironmentProvider environmentProvider) : base(environmentProvider)
        {
        }

        public override string BuildServiceName => "AppVeyor";

        public override string GitHubRepo => Environment
            .GetEnvironmentVariable("APPVEYOR_REPO_NAME")
            .Split(new[] {"/"}, StringSplitOptions.RemoveEmptyEntries)
            .Skip(1)
            .First();

        public override string GitHubOwner => Environment
            .GetEnvironmentVariable("APPVEYOR_REPO_NAME")
            .Split(new[] {"/"}, StringSplitOptions.RemoveEmptyEntries)
            .First();

        public override string CloneRoot => Environment
            .GetEnvironmentVariable("APPVEYOR_BUILD_FOLDER");

        public override string CommitHash => Environment
            .GetEnvironmentVariable("APPVEYOR_REPO_COMMIT");

        public override int? PullRequestNumber => Environment.GetIntEnvironmentVariable("APPVEYOR_PULL_REQUEST_NUMBER");
    }
}