using BCC.MSBuildLog.Model;

namespace BCC.MSBuildLog.Interfaces
{
    public interface IParameterParser
    {
        Parameters Parse(string input);
    }
}