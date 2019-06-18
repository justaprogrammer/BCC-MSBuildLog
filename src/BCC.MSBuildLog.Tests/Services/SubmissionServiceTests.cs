using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BCC.MSBuildLog.Model;
using BCC.MSBuildLog.Services;
using BCC.MSBuildLog.Tests.Util;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RestSharp;
using Xunit;
using Xunit.Abstractions;

namespace BCC.MSBuildLog.Tests.Services
{
    public class SubmissionServiceTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ILogger<SubmissionServiceTests> _logger;

        private static readonly Faker Faker;

        public SubmissionServiceTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _logger = TestLogger.Create<SubmissionServiceTests>(testOutputHelper);
        }

        static SubmissionServiceTests()
        {
            Faker = new Faker();
        }

        [Fact]
        public async Task ShouldSubmit()
        {
            var token = Faker.Random.String();
            var textContents = Faker.Lorem.Paragraph();
            var textContentsBytes = Encoding.Unicode.GetBytes(textContents);
            var headSha = Faker.Random.String();
            int pullRequestNumber = Faker.Random.Int(0);

            var restResponse = Substitute.For<IRestResponse>();
            restResponse.StatusCode.Returns(HttpStatusCode.OK);

            var restClient = Substitute.For<IRestClient>();
            restClient.ExecutePostTaskAsync(Arg.Any<IRestRequest>()).Returns(restResponse);

            var submissionService = new SubmissionService(restClient);

            var result = await submissionService.SubmitAsync(textContentsBytes, new Parameters {Hash = headSha, Token = token, PullRequestNumber = pullRequestNumber});
            result.Should().BeTrue();

            await restClient.Received(1).ExecutePostTaskAsync(Arg.Any<IRestRequest>());
            var objects = restClient.ReceivedCalls().First().GetArguments();

            var restRequest = (RestRequest)objects[0];
            restRequest.Parameters.Should().BeEquivalentTo(
                    new Parameter
                    {
                        Type = ParameterType.HttpHeader,
                        Name = "Authorization",
                        Value = $"Bearer {token}"
                    },
                    new Parameter
                    {
                        Type = ParameterType.RequestBody,
                        Name = "CommitSha",
                        Value = headSha
                    },
                    new Parameter
                    {
                        Type = ParameterType.RequestBody,
                        Name = "PullRequestNumber",
                        Value = pullRequestNumber
                    }
                );

            var restRequestFile = restRequest.Files[0];
            restRequestFile.Name.Should().Be("LogFile");
            restRequestFile.FileName.Should().Be("file.txt");
            restRequestFile.ContentType.Should().BeNull();
            restRequestFile.ContentLength.Should().Be(textContentsBytes.Length);
        }
    }
}