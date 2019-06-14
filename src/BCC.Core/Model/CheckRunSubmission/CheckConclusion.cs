using System.Diagnostics.CodeAnalysis;

namespace BCC.Core.Model.CheckRunSubmission
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum CheckConclusion
    {
        Success,
        Failure,
        Neutral,
        Cancelled,
        TimedOut,
        ActionRequired,
    }
}