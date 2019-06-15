using BCC.MSBuildLog.Interfaces;
using BCC.MSBuildLog.Legacy.MSBuild.Interfaces;
using BCC.MSBuildLog.Legacy.MSBuild.Model;
using BCC.MSBuildLog.Legacy.MSBuild.Services;
using BCC.MSBuildLog.Model;

namespace BCC.MSBuildLog.Services
{
    public class LogDataBuilderFactory : ILogDataBuilderFactory
    {
        public ILogDataBuilder BuildLogDataBuilder(Parameters parameters, CheckRunConfiguration configuration)
        {
            return new LogDataBuilder(parameters.CloneRoot, parameters.Owner, parameters.Repo,
                parameters.Hash, configuration);
        }
    }
}