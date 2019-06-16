using BCC.MSBuildLog.Model;

namespace BCC.MSBuildLog.Interfaces
{
    public interface ILogDataBuilderFactory
    {
        ILogDataBuilder BuildLogDataBuilder(Parameters parameters, CheckRunConfiguration configuration);
    }
}