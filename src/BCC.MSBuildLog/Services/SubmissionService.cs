using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using BCC.MSBuildLog.Interfaces;
using BCC.MSBuildLog.Model;
using RestSharp;

namespace BCC.MSBuildLog.Services
{
    public class SubmissionService : ISubmissionService
    {
        private readonly IRestClient _restClient;

        public SubmissionService(IRestClient restClient)
        {
            _restClient = restClient;
        }

        public async Task<bool> SubmitAsync(byte[] bytes, Parameters parameters)
        {
            if (parameters.PullRequestNumber == null)
            {
                throw new InvalidOperationException("Missing PullRequestNumber");
            }

            var request = new RestRequest("api/checkrun/upload")
            {
                AlwaysMultipartFormData = true,
                RequestFormat = DataFormat.Json,
            };

            Console.WriteLine($"Hash {parameters.Hash}");
            Console.WriteLine($"PullRequestNumber {parameters.PullRequestNumber}");

            request.AddHeader("Authorization", $"Bearer {parameters.Token}");
            request.AddParameter("PullRequestNumber", parameters.PullRequestNumber.Value, ParameterType.RequestBody);
            request.AddParameter("CommitSha", parameters.Hash, ParameterType.RequestBody);
            request.AddFile("LogFile", bytes, "file.txt");

            var restResponse = await _restClient.ExecutePostTaskAsync(request)
                .ConfigureAwait(false);

            Console.WriteLine("Response: {0}", restResponse.StatusCode.ToString());

            return restResponse.StatusCode == HttpStatusCode.OK;
        }
    }
}