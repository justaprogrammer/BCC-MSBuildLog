using BCC.MSBuildLog.Legacy.MSBuild.Interfaces;
using BCC.MSBuildLog.Legacy.MSBuild.Model;
using BCC.MSBuildLog.Model;

namespace BCC.MSBuildLog.Interfaces
{
    public interface ILogDataBuilderFactory
    {
        ILogDataBuilder BuildLogDataBuilder(Parameters parameters, CheckRunConfiguration configuration);
    }
}