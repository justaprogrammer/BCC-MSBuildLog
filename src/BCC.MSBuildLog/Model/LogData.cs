using BCC.Core.Model.CheckRunSubmission;

namespace BCC.MSBuildLog.Model
{
    public class LogData
    {
        public int MessageCount { get; set; }
        public int WarningCount { get; set; }
        public int ErrorCount { get; set; }
        public Annotation[] Annotations { get; set; }
        public string Report { get; set; }
    }
}