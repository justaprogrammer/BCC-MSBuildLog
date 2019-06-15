using BCC.MSBuildLog.Model;
using Microsoft.Build.Framework;

namespace BCC.MSBuildLog.Services
{
    public interface ILogDataBuilder
    {
        LogData Build();
        void ProcessRecord(BuildEventArgs recordArgs);
    }
}