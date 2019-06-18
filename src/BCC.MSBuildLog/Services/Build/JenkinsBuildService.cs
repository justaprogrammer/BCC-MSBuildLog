using BCC.MSBuildLog.Interfaces;

namespace BCC.MSBuildLog.Services.Build
{
    /// <summary>
    /// Wrapper of Jenkins environment information
    /// https://wiki.jenkins.io/display/JENKINS/Building+a+software+project#Buildingasoftwareproject-belowJenkinsSetEnvironmentVariables
    /// </summary>
    public class JenkinsBuildService : BuildServiceBase
    {
        public JenkinsBuildService(IEnvironmentProvider environmentProvider) : base(environmentProvider)
        {
        }

        public override string BuildServiceName => "Jenkins";

        public override string GitHubRepo => null;

        public override string GitHubOwner => null;

        public override string CloneRoot => Environment.GetEnvironmentVariable("WORKSPACE");

        public override string CommitHash => Environment.GetEnvironmentVariable("GIT_COMMIT");

        public override int? PullRequestNumber => null;
    }
}