namespace BCC.MSBuildLog.Logger
{
    public interface IEnvironment
    {
        string GetEnvironmentVariable(string name);

        void WriteLine(string line);
    }
}