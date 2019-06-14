using BCC.Core.Model.CheckRunSubmission;
using BCC.Core.Serialization;

namespace BCC.Core.Extensions
{
    public static class CreateCheckRunExtensions
    {
        public static string ToJson(this CreateCheckRun createCheckRun) => CreateCheckRunSerializer.Serialize(createCheckRun);
    }
}
