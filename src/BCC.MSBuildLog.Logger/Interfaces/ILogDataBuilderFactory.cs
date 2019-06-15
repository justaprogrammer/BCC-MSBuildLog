using BCC.MSBuildLog.Logger.Model;
using BCC.MSBuildLog.Model;
using BCC.MSBuildLog.Services;

namespace BCC.MSBuildLog.Logger.Interfaces
{
    public interface ILogDataBuilderFactory
    {
        ILogDataBuilder BuildLogDataBuilder(Parameters parameters, CheckRunConfiguration configuration);
    }
}