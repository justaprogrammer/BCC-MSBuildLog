using BCC.MSBuildLog.Legacy.MSBuild.Model;
using JetBrains.Annotations;

namespace BCC.MSBuildLog.Legacy.MSBuild.Interfaces
{
    /// <summary>
    /// This service reads a binary log file and outputs captured information.
    /// </summary>
    public interface IBinaryLogProcessor
    {
        /// <summary>
        /// Reads a binary log file and outputs captured information.
        /// </summary>
        /// <param name="binLogPath">The location of the (.binlog) file. Binary log files always have the extension binlog. MSBuild won't let you do it any other way.</param>
        /// <param name="cloneRoot">The location that the build was performed from. This assumes that the build path was a child of cloneRoot.</param>
        /// <param name="owner"></param>
        /// <param name="repo"></param>
        /// <param name="hash"></param>
        /// <param name="configuration">The check run configuration to follow.</param>
        /// <returns>A <see cref="LogData"/> object.</returns>
        LogData ProcessLog([NotNull] string binLogPath, [NotNull] string cloneRoot, string owner, string repo, string hash, CheckRunConfiguration configuration = null);
    }
}