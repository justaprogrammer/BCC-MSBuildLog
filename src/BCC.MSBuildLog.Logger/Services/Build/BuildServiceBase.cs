using BCC.MSBuildLog.Logger.Interfaces;
using BCC.MSBuildLog.Logger.Interfaces.Build;

namespace BCC.MSBuildLog.Logger.Services.Build
{
    public abstract class BuildServiceBase : IBuildService
    {
        protected IEnvironmentProvider Environment { get; }

        public BuildServiceBase(IEnvironmentProvider environmentProvider)
        {
            Environment = environmentProvider;
        }

        public abstract string GitHubRepo { get; }
        public abstract string GitHubOwner { get; }
        public abstract string CloneRoot { get; }
        public abstract string CommitHash { get; }

        public string BuildCrossCheckToken => Environment
            .GetEnvironmentVariable("BCC_TOKEN");
    }
}