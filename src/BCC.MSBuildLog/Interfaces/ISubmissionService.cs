using System.Threading.Tasks;
using BCC.MSBuildLog.Model;

namespace BCC.MSBuildLog.Interfaces
{
    public interface ISubmissionService
    {
        Task<bool> SubmitAsync(byte[] bytes, Parameters parameters);
    }
}