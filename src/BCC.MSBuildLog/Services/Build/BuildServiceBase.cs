using BCC.MSBuildLog.Interfaces;
using BCC.MSBuildLog.Interfaces.Build;

namespace BCC.MSBuildLog.Services.Build
{
    public abstract class BuildServiceBase : IBuildService
    {
        protected IEnvironmentProvider Environment { get; }

        public BuildServiceBase(IEnvironmentProvider environmentProvider)
        {
            Environment = environmentProvider;
        }

        public abstract string BuildServiceName { get; }
        public abstract string GitHubRepo { get; }
        public abstract string GitHubOwner { get; }
        public abstract string CloneRoot { get; }
        public abstract string CommitHash { get; }
    }
}