namespace BCC.MSBuildLog.Interfaces
{
    public interface IBuildLogProcessor
    {
        void Proces(string inputFile, string outputFile, string cloneRoot, string owner, string repo, string hash, string configurationFile = null);
    }
}