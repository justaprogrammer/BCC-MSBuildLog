namespace BCC.MSBuildLog.Logger.Interfaces.Build
{
    public interface IBuildService
    {
        string GitHubRepo { get; }
        string GitHubOwner { get; }
        string CloneRoot { get; }
        string CommitHash { get; }
        string BuildCrossCheckToken { get; }
    }
}