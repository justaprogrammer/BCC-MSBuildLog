namespace BCC.MSBuildLog.Legacy.MSBuild.Interfaces
{
    public interface ICommandLineParser
    {
        ApplicationArguments Parse(string[] args);
    }
}