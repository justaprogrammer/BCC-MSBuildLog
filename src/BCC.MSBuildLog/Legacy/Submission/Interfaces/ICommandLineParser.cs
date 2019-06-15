namespace BCC.MSBuildLog.Legacy.Submission.Interfaces
{
    public interface ICommandLineParser
    {
        ApplicationArguments Parse(string[] args);
    }
}