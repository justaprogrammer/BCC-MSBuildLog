using BCC.MSBuildLog.Logger.Interfaces;
using BCC.MSBuildLog.Logger.Model;
using BCC.MSBuildLog.Model;
using BCC.MSBuildLog.Services;

namespace BCC.MSBuildLog.Logger.Services
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