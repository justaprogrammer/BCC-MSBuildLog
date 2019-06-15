using BCC.MSBuildLog.Legacy.MSBuild.Model;
using Microsoft.Build.Framework;

namespace BCC.MSBuildLog.Legacy.MSBuild.Interfaces
{
    public interface ILogDataBuilder
    {
        LogData Build();
        void ProcessRecord(BuildEventArgs recordArgs);
    }
}