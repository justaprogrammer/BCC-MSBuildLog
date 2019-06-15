namespace BCC.MSBuildLog.Logger.Interfaces
{
    public interface IEnvironmentProvider
    {
        string GetEnvironmentVariable(string name);

        void WriteLine(string line);

        void DebugLine(string line);
    }
}