using BCC.MSBuildLog.Model;
using Microsoft.Build.Framework;

namespace BCC.MSBuildLog.Interfaces
{
    public interface ILogDataBuilder
    {
        LogData Build();
        void ProcessRecord(BuildEventArgs recordArgs);
    }
}