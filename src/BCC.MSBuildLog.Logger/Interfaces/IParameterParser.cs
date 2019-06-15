using BCC.MSBuildLog.Logger.Model;
using BCC.MSBuildLog.Logger.Services;

namespace BCC.MSBuildLog.Logger.Interfaces
{
    public interface IParameterParser
    {
        Parameters Parse(string input);
    }
}