namespace BCC.MSBuildLog.Interfaces
{
    public interface IEnvironmentProvider
    {
        string GetEnvironmentVariable(string name);

        int? GetIntEnvironmentVariable(string name);

        void WriteLine(string line);

        void DebugLine(string line);
    }
}