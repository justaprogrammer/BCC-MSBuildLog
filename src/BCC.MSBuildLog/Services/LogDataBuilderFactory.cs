using BCC.MSBuildLog.Interfaces;
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