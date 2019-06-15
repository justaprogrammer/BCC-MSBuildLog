namespace BCC.MSBuildLog.Legacy.MSBuild.Interfaces
{
    public interface IBuildLogProcessor
    {
        void Process(string inputFile, string outputFile, string cloneRoot, string owner, string repo, string hash, string configurationFile = null);
    }
}