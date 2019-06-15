using System;
using BCC.MSBuildLog.Logger.Interfaces;
using BCC.MSBuildLog.Logger.Interfaces.Build;

namespace BCC.MSBuildLog.Logger.Services.Build
{
    public class BuildServiceProvider: IBuildServiceProvider
    {
        private readonly IEnvironmentProvider _environmentProvider;

        public BuildServiceProvider(IEnvironmentProvider environmentProvider)
        {
            _environmentProvider = environmentProvider;
        }

        public IBuildService GetBuildService()
        {
            if (!string.IsNullOrWhiteSpace(_environmentProvider.GetEnvironmentVariable("APPVEYOR")))
            {
                return new AppVeyorBuildService(_environmentProvider);
            }

            if (!string.IsNullOrWhiteSpace(_environmentProvider.GetEnvironmentVariable("TRAVIS")))
            {
                return new TravisBuildService(_environmentProvider);
            }

            if (!string.IsNullOrWhiteSpace(_environmentProvider.GetEnvironmentVariable("CIRCLECI")))
            {
                return new CircleBuildService(_environmentProvider);
            }

            if (!string.IsNullOrWhiteSpace(_environmentProvider.GetEnvironmentVariable("JENKINS_HOME")))
            {
                return new JenkinsBuildService(_environmentProvider);
            }

            throw new EnvironmentServiceNotFoundException();
        }
    }

    public class EnvironmentServiceNotFoundException : Exception
    {
    }
}
